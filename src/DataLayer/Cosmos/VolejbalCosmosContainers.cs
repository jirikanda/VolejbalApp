using Microsoft.Azure.Cosmos;

namespace KandaEu.Volejbal.DataLayer.Cosmos;

/// <summary>
/// Kontejnery aplikace. Názvy a partition key jsou pevné - stejné hodnoty používá infra/main.bicep
/// (Azure) i <see cref="CosmosDatabaseInitializer" /> (emulátor); rozjedou-li se, dotazy s partition key
/// začnou vracet prázdné výsledky.
/// </summary>
public class VolejbalCosmosContainers
{
	public const string OsobyContainerId = "osoby";
	public const string TerminyContainerId = "terminy";
	public const string VzkazyContainerId = "vzkazy";

	public const string OsobyPartitionKeyPath = "/id";
	public const string TerminyPartitionKeyPath = "/id";
	public const string VzkazyPartitionKeyPath = "/id";

	public Container Osoby { get; }
	public Container Terminy { get; }
	public Container Vzkazy { get; }

	public VolejbalCosmosContainers(CosmosClient cosmosClient, CosmosOptions options)
	{
		// GetDatabase/GetContainer jsou jen lokální odkazy, nic se nevolá - kontejnery musí existovat
		// (bicep v Azure, MigrationTool lokálně), aplikace je za běhu nezakládá.
		Database database = cosmosClient.GetDatabase(options.DatabaseId);
		Osoby = database.GetContainer(OsobyContainerId);
		Terminy = database.GetContainer(TerminyContainerId);
		Vzkazy = database.GetContainer(VzkazyContainerId);
	}
}
