using Havit.Data.EntityFrameworkCore;
using Havit.Data.Patterns.DataSeeds;
using KandaEu.Volejbal.DataLayer.Seeds.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KandaEu.Volejbal.Services.Infrastructure;

public class DatabaseMigrationHostedService(
	IServiceScopeFactory _serviceScopeFactory) : IHostedService
{ 
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using (IServiceScope serviceScope = _serviceScopeFactory.CreateScope())
		{
			var context = serviceScope.ServiceProvider.GetService<IDbContext>();
			await context.Database.MigrateAsync(cancellationToken);

			var dataSeedRunner = serviceScope.ServiceProvider.GetService<IDataSeedRunner>();
			await dataSeedRunner.SeedDataAsync<CoreProfile>(cancellationToken: cancellationToken);
		}
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		// NOOP
		return Task.CompletedTask;
	}
}