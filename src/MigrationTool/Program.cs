using KandaEu.Volejbal.DataLayer.Cosmos;
using KandaEu.Volejbal.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.MigrationTool;

/// <summary>
/// Příprava datového úložiště: založí databázi a kontejnery, pokud ještě nejsou, a volitelně převede
/// data mezi Cosmos DB a původní SQL databází (oběma směry).
/// </summary>
/// <remarks>
/// Spouští se ručně z lokálního klonu, CI/CD se ho nedotýká. V produkci datové úložiště zakládá
/// infra/main.bicep, takže tam je tool potřeba jen kvůli převodu dat; lokálně (proti emulátoru) je to
/// jediná cesta, jak databázi vytvořit.
/// </remarks>
public class Program
{
	public static async Task Main(string[] args)
	{
		IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration((hostContext, config) =>
			{
				config
					.AddCommandLine(args, new Dictionary<string, string>
					{
						{ "--endpoint", "Cosmos:Endpoint" },
						{ "--key", "Cosmos:Key" },
						{ "--database", "Cosmos:DatabaseId" },
						{ "--importfromsql", "ImportFromSql" },
						{ "--exporttosql", "ExportToSql" },
					})
					.AddEnvironmentVariables();
			})
			.ConfigureLogging(logging =>
			{
				logging.AddSimpleConsole(configure => configure.TimestampFormat = "[HH:mm:ss] ");
			})
			.ConfigureServices((hostContext, services) =>
			{
				services.ConfigureForMigrationTool(hostContext.Configuration);
			});

		IHost host = hostBuilder.Build();

		IConfiguration configuration = host.Services.GetRequiredService<IConfiguration>();
		ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();

		string importFromSql = configuration["ImportFromSql"];
		string exportToSql = configuration["ExportToSql"];

		if (!String.IsNullOrEmpty(importFromSql) && !String.IsNullOrEmpty(exportToSql))
		{
			logger.LogError("Zadej buď --importfromsql, nebo --exporttosql, ne obojí najednou.");
			return;
		}

		if (!String.IsNullOrEmpty(exportToSql))
		{
			// Cosmos je zdroj, ne cíl - kontejnery se nezakládají.
			await new CosmosExport(
				host.Services.GetRequiredService<VolejbalCosmosContainers>(),
				host.Services.GetRequiredService<ILogger<CosmosExport>>())
				.ExportAsync(exportToSql);
			return;
		}

		logger.LogInformation("Zakládám databázi a kontejnery (pokud ještě nejsou)...");
		await host.Services.GetRequiredService<CosmosDatabaseInitializer>().EnsureDatabaseAsync();
		logger.LogInformation("Hotovo.");

		if (!String.IsNullOrEmpty(importFromSql))
		{
			await new SqlImport(
				host.Services.GetRequiredService<VolejbalCosmosContainers>(),
				host.Services.GetRequiredService<ILogger<SqlImport>>())
				.ImportAsync(importFromSql);
		}
		else
		{
			logger.LogInformation("Parametr --importfromsql nezadán, převod dat se přeskakuje.");
		}
	}
}
