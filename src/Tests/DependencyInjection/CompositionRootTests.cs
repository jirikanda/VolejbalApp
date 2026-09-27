using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KandaEu.Volejbal.Tests.DependencyInjection;

/// <summary>
/// Sestaví produkční DI kontejner a z něj vyzvedne každou fasádu (I*Api).
/// </summary>
/// <remarks>
/// Chytá chyby, které kompilátor nevidí a které se dřív projevily až v nasazené aplikaci jako
/// "0 functions found" a restartující se worker: fasáda bez <c>[Service(ServiceType = typeof(I*Api))]</c>,
/// služba s nezaregistrovanou závislostí, chybějící registrace v DataLayer. Databázi nepotřebuje -
/// konstruktor CosmosClient žádné I/O nedělá.
/// </remarks>
[TestClass]
public class CompositionRootTests
{
	[TestMethod]
	public void ConfigureForWebAPI_KazdaFasadaJdeVyzvednoutZProdukcnihoKontejneru()
	{
		// Arrange - produkční tvar konfigurace: prázdný klíč = managed identita.
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string>
			{
				["Cosmos:Endpoint"] = "https://localhost:8081/",
				["Cosmos:Key"] = "",
				["Cosmos:DatabaseId"] = "volejbal"
			})
			.Build();

		ServiceCollection services = new ServiceCollection();
		// Totéž, co v Api/Program.cs přidává hostitel mimo ConfigureForWebAPI.
		services.AddLogging();
		services.ConfigureForWebAPI(configuration);

		Type[] apiInterfaces = GetApiInterfaces();
		Assert.IsNotEmpty(apiInterfaces, "V Contracts se nenašlo žádné I*Api rozhraní - test by nic nehlídal.");

		// Act
		using ServiceProvider serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
		{
			ValidateOnBuild = true,
			ValidateScopes = true
		});
		using IServiceScope scope = serviceProvider.CreateScope();

		List<string> chybejici = new List<string>();
		foreach (Type apiInterface in apiInterfaces)
		{
			if (scope.ServiceProvider.GetService(apiInterface) == null)
			{
				chybejici.Add(apiInterface.Name);
			}
		}

		// Assert
		Assert.IsEmpty(chybejici, $"Tyto fasády nejde z kontejneru vyzvednout (chybí [Service(ServiceType = typeof(I*Api))]?):{Environment.NewLine}{String.Join(Environment.NewLine, chybejici)}");
	}

	/// <summary>
	/// Všechna rozhraní I*Api z Contracts - přibude-li nová oblast API, test ji hlídá automaticky.
	/// </summary>
	private static Type[] GetApiInterfaces()
	{
		return typeof(ApiRoutes).Assembly.GetTypes()
			.Where(type => type.IsInterface && type.IsPublic && type.Name.EndsWith("Api", StringComparison.Ordinal))
			.OrderBy(type => type.Name, StringComparer.Ordinal)
			.ToArray();
	}
}
