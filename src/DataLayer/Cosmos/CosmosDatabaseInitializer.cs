using Microsoft.Azure.Cosmos;

namespace KandaEu.Volejbal.DataLayer.Cosmos;

/// <summary>
/// Založí databázi a kontejnery, pokud ještě nejsou. Používá jen MigrationTool (lokální emulátor)
/// a TestsForLocalDebugging - v Azure zakládá totéž infra/main.bicep a aplikace ho nevolá, aby
/// neprodlužovala studený start.
/// </summary>
/// <remarks>
/// Definice kontejnerů (partition key, indexy) musí zůstat shodná s bicep šablonou.
/// </remarks>
public class CosmosDatabaseInitializer(CosmosClient _cosmosClient, CosmosOptions _options)
{
	/// <summary>
	/// Propustnost sdílená celou databází, na minimu (400 RU/s stačí až pro 4 kontejnery). Stejná hodnota
	/// jako cosmosThroughput v infra/main.bicep; zbytek free tier grantu (1000 RU/s) zůstává volný pro
	/// další databázi. Tři samostatně provisionované kontejnery by chtěly 3x400 RU/s a nevešly by se.
	/// </summary>
	public const int SdilenaPropustnostRu = 400;

	public async Task EnsureDatabaseAsync(CancellationToken cancellationToken = default)
	{
		DatabaseResponse databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
			_options.DatabaseId,
			ThroughputProperties.CreateManualThroughput(SdilenaPropustnostRu),
			cancellationToken: cancellationToken);
		Database database = databaseResponse.Database;

		await database.DefineContainer(VolejbalCosmosContainers.OsobyContainerId, VolejbalCosmosContainers.OsobyPartitionKeyPath)
			.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

		// Přihlášky jsou vnořené v dokumentu termínu a z indexu vyloučené: dovnitř pole se nikdy
		// nedotazujeme, ale každé přihlášení dokument přepíše - bez vyloučení by se při nejčastější
		// operaci aplikace přeindexovalo celé pole.
		await database.DefineContainer(VolejbalCosmosContainers.TerminyContainerId, VolejbalCosmosContainers.TerminyPartitionKeyPath)
			.WithIndexingPolicy()
				.WithIncludedPaths()
					.Path("/*")
					.Attach()
				.WithExcludedPaths()
					.Path("/prihlasky/*")
					.Path("/\"_etag\"/?")
					.Attach()
				.Attach()
			.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

		await database.DefineContainer(VolejbalCosmosContainers.VzkazyContainerId, VolejbalCosmosContainers.VzkazyPartitionKeyPath)
			.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
	}
}
