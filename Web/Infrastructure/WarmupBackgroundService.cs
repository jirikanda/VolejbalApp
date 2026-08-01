using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace KandaEu.Volejbal.Web.Infrastructure;

/// <summary>
/// Po startu zavolá vlastní loopback HTTP požadavky na endpointy, které reálně obsluhují úvodní stránku
/// (Home + NastenkaSidebar), aby se ještě předtím, než dorazí první request od uživatele, zahřálo vše,
/// co cold cestu prodlužuje — nejen EF Core (sestavení modelu, zkompilování konkrétního tvaru LINQ dotazu,
/// SQL connection pool), ale i MVC pipeline (routing, aktivace controlleru, JSON serializace, rate limiter
/// middleware). Volání přímo facády by MVC vrstvu obešlo a nic z toho by nezahřálo — proto jde přes HTTP.
///
/// Důležité při scale-to-zero na ACA. Čeká na <see cref="IHostApplicationLifetime.ApplicationStarted"/>,
/// protože teprve potom má Kestrel svázaný poslouchací port.
/// </summary>
public class WarmupBackgroundService(
	IServer _server,
	IHostApplicationLifetime _lifetime,
	ILogger<WarmupBackgroundService> _logger) : BackgroundService
{
	private static readonly string[] s_warmupPaths = ["/api/nastenka", "/api/osoby/aktivni", "/api/terminy"];

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!await WaitForApplicationStartedAsync(stoppingToken))
		{
			return; // shutdown dřív, než aplikace naběhla
		}

		Uri baseAddress = GetLoopbackBaseAddress();
		if (baseAddress == null)
		{
			_logger.LogWarning("Warmup přeskočen — nepodařilo se zjistit adresu, na které Kestrel naslouchá.");
			return;
		}

		try
		{
			using HttpClient httpClient = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(30) };
			foreach (string path in s_warmupPaths)
			{
				await httpClient.GetAsync(path, stoppingToken);
			}
			_logger.LogInformation("Warmup dokončen.");
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			// shutdown aplikace — nejde o chybu
		}
		catch (Exception exception)
		{
			// Selhání warmupu nevadí — první reálný request si vše zahřeje sám, jen o něco pomaleji.
			_logger.LogWarning(exception, "Warmup selhal.");
		}
	}

	private async Task<bool> WaitForApplicationStartedAsync(CancellationToken stoppingToken)
	{
		using CancellationTokenSource combined = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _lifetime.ApplicationStarted);
		try
		{
			await Task.Delay(Timeout.Infinite, combined.Token);
		}
		catch (OperationCanceledException)
		{
			// očekávané — buď aplikace naběhla (ApplicationStarted), nebo přišel shutdown (stoppingToken)
		}
		return !stoppingToken.IsCancellationRequested;
	}

	private Uri GetLoopbackBaseAddress()
	{
		string address = _server.Features.Get<IServerAddressesFeature>()?.Addresses
			.FirstOrDefault(a => a.StartsWith("http://", StringComparison.OrdinalIgnoreCase));
		if (address == null)
		{
			return null;
		}

		// V kontejneru Kestrel naslouchá na wildcard hostu (např. "http://+:8080" nebo "http://[::]:8080") —
		// pro loopback volání potřebujeme konkrétní adresu.
		address = address.Replace("://+", "://localhost").Replace("://*", "://localhost")
			.Replace("://0.0.0.0", "://localhost").Replace("://[::]", "://localhost");

		return Uri.TryCreate(address, UriKind.Absolute, out Uri uri) ? uri : null;
	}
}
