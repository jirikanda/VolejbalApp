using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Services.Jobs;

/// <summary>
/// Plánovač opakovaných úloh (náhrada Hangfire — ten měl in-memory storage, takže stejně nepřežíval restart,
/// a jeho inicializace prodlužovala start aplikace, což se počítá při scale-to-zero cold startech na ACA).
/// Běží na pozadí (BackgroundService + Task.Yield), start webserveru neblokuje.
///
/// - Při startu: EnsureTerminy (materializace termínů).
/// - Každou hodinu běhu: EnsureTerminy.
///
/// Startovní běh mimochodem zahřeje EF Core model a connection pool, takže je první request o tuhle
/// položku levnější. Za warmup to ale považovat nelze: job jde přímo přes službu, ne přes Kestrel,
/// takže routing, aktivace controlleru, serializace DTO ani compiled-query cache pro tvary dotazů,
/// které API opravdu používá, zahřáté nejsou. Naměřeno ~100 ms na první request po probuzení
/// (dřívější WarmupBackgroundService s loopback HTTP requesty ho stahovala na ~5–40 ms).
/// Podrobnosti a důvod odstranění viz deploy/README.md, sekce k minReplicas: 0.
///
/// Poznámka: <see cref="IDeaktivaceOsobJob"/> se záměrně neplánuje — osoby se deaktivují ručně
/// ve správě hráčů, automatická deaktivace po dvou měsících neúčasti byla zrušena.
/// Kód jobu zůstává k dispozici pro případné ruční/jednorázové použití.
/// </summary>
public class RecurringJobsBackgroundService(
	IServiceProvider _serviceProvider,
	ILogger<RecurringJobsBackgroundService> _logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Yield(); // uvolníme startup — zbytek běží na pozadí, aplikace mezitím začne obsluhovat requesty

		await RunJobAsync<IEnsureTerminyJob>(stoppingToken);

		using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(1));
		while (await timer.WaitForNextTickAsync(stoppingToken))
		{
			await RunJobAsync<IEnsureTerminyJob>(stoppingToken);
		}
	}

	private async Task RunJobAsync<TJob>(CancellationToken cancellationToken)
		where TJob : IRunnableJob
	{
		try
		{
			_logger.LogInformation("Spouštím job {job}...", typeof(TJob).Name);
			using IServiceScope scope = _serviceProvider.CreateScope();
			await scope.ServiceProvider.GetRequiredService<TJob>().ExecuteAsync(cancellationToken);
			_logger.LogInformation("Job {job} dokončen.", typeof(TJob).Name);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw; // shutdown aplikace — nejde o chybu jobu
		}
		catch (Exception exception)
		{
			// Selhání jednoho běhu nesmí shodit plánovač — job se spustí zase v dalším termínu.
			_logger.LogError(exception, "Job {job} selhal.", typeof(TJob).Name);
		}
	}
}
