using Havit.Data.EntityFrameworkCore;
using Havit.Data.Patterns.DataSeeds;
using KandaEu.Volejbal.DataLayer.Seeds.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KandaEu.Volejbal.Services.Infrastructure.MigrationTool;

public class MigrationService(
	IServiceScopeFactory _serviceScopeFactory,
	IConfiguration _configuration) : IMigrationService
{
	public async Task UpgradeDatabaseSchemaAndDataAsync(CancellationToken cancellationToken = default)
	{
		using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();

		IDbContext context = serviceScope.ServiceProvider.GetRequiredService<IDbContext>();

		context.Database.SetCommandTimeout(TimeSpan.FromSeconds(_configuration.GetValue<int?>("AppSettings:Migrations:CommandTimeout") ?? 300));
		await context.Database.MigrateAsync(cancellationToken);

		IDataSeedRunner dataSeedRunner = serviceScope.ServiceProvider.GetRequiredService<IDataSeedRunner>();
		await dataSeedRunner.SeedDataAsync<CoreProfile>(cancellationToken: cancellationToken);
	}
}
