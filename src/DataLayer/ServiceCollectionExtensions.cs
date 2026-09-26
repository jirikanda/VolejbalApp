using KandaEu.Volejbal.DataLayer.Cosmos;
using KandaEu.Volejbal.DataLayer.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace KandaEu.Volejbal.DataLayer;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registrace datové vrstvy: klient Cosmos DB, kontejnery a repozitáře.
	/// </summary>
	/// <remarks>
	/// Vše jako singleton. <see cref="CosmosClient" /> je drahý (drží spojení a cache adres) - scoped
	/// registrace by na Flex Consumption navazovala spojení při každém požadavku. Repozitáře jsou
	/// bezstavové, takže mohou klienta sdílet.
	/// </remarks>
	public static IServiceCollection AddDataLayerServices(this IServiceCollection services, CosmosOptions cosmosOptions)
	{
		ArgumentNullException.ThrowIfNull(cosmosOptions);

		services.AddSingleton(cosmosOptions);
		services.AddSingleton(serviceProvider => CosmosClientFactory.Create(serviceProvider.GetRequiredService<CosmosOptions>()));
		services.AddSingleton<VolejbalCosmosContainers>();
		services.AddSingleton<CosmosDatabaseInitializer>();

		services.AddSingleton<IOsobaRepository, OsobaCosmosRepository>();
		services.AddSingleton<ITerminRepository, TerminCosmosRepository>();
		services.AddSingleton<IVzkazRepository, VzkazCosmosRepository>();

		return services;
	}
}
