using System.Net;
using KandaEu.Volejbal.DataLayer.Cosmos;
using KandaEu.Volejbal.DependencyInjection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KandaEu.Volejbal.TestsForLocalDebugging;

/// <summary>
/// Bázová třída pro testy. Zpřístupňuje nakonfigurovaný DI container nad lokálním Cosmos DB emulátorem
/// a před každým testem databázi maže a zakládá znovu.
/// </summary>
/// <remarks>
/// Vyžaduje běžící Cosmos DB Emulator (viz appsettings.json). Do CI tyhle testy nepatří - proto jsou
/// v samostatném projektu a označené [Ignore].
/// </remarks>
public class TestBase
{
	protected IServiceProvider ServiceProvider { get; private set; }

	[TestInitialize]
	public virtual async Task TestInitializeAsync()
	{
		IServiceCollection services = new ServiceCollection();

		// Co v hostiteli (Api/MigrationTool) přidá generic host sám, tady musíme dodat ručně - bez toho
		// se nedá sestavit ILogger<T>.
		services.AddLogging();

		services.ConfigureForTests();

		IServiceProvider serviceProvider = services.BuildServiceProvider();

		CosmosOptions cosmosOptions = serviceProvider.GetRequiredService<CosmosOptions>();
		CosmosClient cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();

		try
		{
			await cosmosClient.GetDatabase(cosmosOptions.DatabaseId).DeleteAsync();
		}
		catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
		{
			// Databáze ještě není, nic k mazání.
		}

		await serviceProvider.GetRequiredService<CosmosDatabaseInitializer>().EnsureDatabaseAsync();

		this.ServiceProvider = serviceProvider;
	}

	[TestCleanup]
	public virtual void TestCleanup()
	{
		if (this.ServiceProvider is IDisposable disposable)
		{
			disposable.Dispose();
		}
		this.ServiceProvider = null;
	}
}
