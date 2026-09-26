using KandaEu.Volejbal.DataLayer.Cosmos;

namespace KandaEu.Volejbal.DependencyInjection;

internal class InstallConfiguration
{
	public CosmosOptions CosmosOptions { get; set; }
	public string[] ServiceProfiles { get; set; }
}
