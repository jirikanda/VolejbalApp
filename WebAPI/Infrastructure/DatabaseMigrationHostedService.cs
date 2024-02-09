using Havit.Data.EntityFrameworkCore;
using Havit.Data.Patterns.DataSeeds;
using KandaEu.Volejbal.DataLayer.Seeds.Core;
using Microsoft.EntityFrameworkCore;

namespace KandaEu.Volejbal.WebAPI.Infrastructure;

public class DatabaseMigrationHostedService : IHostedService
{
	private readonly IServiceScopeFactory serviceScopeFactory;

	public DatabaseMigrationHostedService(IServiceScopeFactory serviceScopeFactory)
	{
		this.serviceScopeFactory = serviceScopeFactory;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
		{
			var context = serviceScope.ServiceProvider.GetService<IDbContext>();
			context.Database.Migrate();

			var dataSeedRunner = serviceScope.ServiceProvider.GetService<IDataSeedRunner>();
			dataSeedRunner.SeedData<CoreProfile>();
		}
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		// NOOP
		return Task.CompletedTask;
	}
}