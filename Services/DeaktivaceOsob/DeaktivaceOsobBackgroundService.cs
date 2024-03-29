﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KandaEu.Volejbal.Services.DeaktivaceOsob;

public class DeaktivaceOsobBackgroundService (IServiceProvider _serviceProvider) : BackgroundService
{ 
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Delay(15000, stoppingToken); // workaround: necháme aplikaci nastartovat a spustit migrace
		while (!stoppingToken.IsCancellationRequested)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var deaktivaceOsobService = scope.ServiceProvider.GetRequiredService<IDeaktivaceOsobService>();
				await deaktivaceOsobService.DeaktivujOsobyAsync(stoppingToken);
			}

			await Task.Delay(1000 * 60 * 60 * 6 /* 6 hodiny */, stoppingToken);
		}
	}
}
