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
/// - Při startu: EnsureTerminy (materializace termínů) + DeaktivaceOsob (idempotentní catch-up —
///   při scale-to-zero aplikace ve 4:00 typicky spí, úloha se dožene při nejbližším probuzení).
/// - Každou hodinu běhu: EnsureTerminy.
/// - Jednou denně po 4:00 místního času (pokud aplikace zrovna běží): DeaktivaceOsob.
/// </summary>
public class RecurringJobsBackgroundService(
	IServiceProvider _serviceProvider,
	ILogger<RecurringJobsBackgroundService> _logger) : BackgroundService
{
	private static readonly TimeZoneInfo s_timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Yield(); // uvolníme startup — zbytek běží na pozadí, aplikace mezitím začne obsluhovat requesty

		await RunJobAsync<IEnsureTerminyJob>(stoppingToken);
		await RunJobAsync<IDeaktivaceOsobJob>(stoppingToken);
		DateTime lastDeaktivaceDate = GetCurrentLocalTime().Date;

		using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromHours(1));
		while (await timer.WaitForNextTickAsync(stoppingToken))
		{
			await RunJobAsync<IEnsureTerminyJob>(stoppingToken);

			DateTime now = GetCurrentLocalTime();
			if ((now.Date > lastDeaktivaceDate) && (now.Hour >= 4))
			{
				await RunJobAsync<IDeaktivaceOsobJob>(stoppingToken);
				lastDeaktivaceDate = now.Date;
			}
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

	private static DateTime GetCurrentLocalTime() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, s_timeZone);
}
