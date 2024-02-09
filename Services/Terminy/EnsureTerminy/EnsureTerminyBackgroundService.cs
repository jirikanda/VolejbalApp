using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

public class EnsureTerminyBackgroundService(
	IServiceProvider _serviceProvider) : BackgroundService
{

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Delay(15000, stoppingToken); // workaround: necháme aplikaci nastartovat a spustit migrace
		while (!stoppingToken.IsCancellationRequested)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var ensureTerminyService = scope.ServiceProvider.GetRequiredService<IEnsureTerminyService>();
				await ensureTerminyService.EnsureTerminyAsync(stoppingToken);
			}

			await Task.Delay(1000 * 60 * 60 /* 1 hodina */, stoppingToken);
		}
	}
}
