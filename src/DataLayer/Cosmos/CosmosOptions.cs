namespace KandaEu.Volejbal.DataLayer.Cosmos;

/// <summary>
/// Připojení k Cosmos DB (sekce "Cosmos" v konfiguraci).
/// </summary>
public class CosmosOptions
{
	/// <summary>
	/// Endpoint účtu, např. "https://jkvolejbalcosmosdb.documents.azure.com:443/" nebo emulátor "https://localhost:8081/".
	/// </summary>
	public string Endpoint { get; set; }

	/// <summary>
	/// Klíč účtu. Prázdný = přihlášení přes <c>DefaultAzureCredential</c> (managed identita v Azure,
	/// az login lokálně). Vyplňuje se jen pro emulátor, jehož klíč je veřejně známá konstanta.
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// Název databáze. Bez defaultu - každé prostředí ho nastavuje výslovně (bicep app setting, appsettings
	/// pro Development, parametr --database u MigrationToolu). Kontejnery mají pevné názvy, viz
	/// <see cref="VolejbalCosmosContainers" />.
	/// </summary>
	public string DatabaseId { get; set; }

	/// <summary>
	/// Ověří, že je vyplněné vše, bez čeho se klient nedá sestavit. Volá <see cref="CosmosClientFactory" />,
	/// takže chybějící hodnota shodí aplikaci při prvním sáhnutí na data se srozumitelnou hláškou,
	/// ne až někde v SDK.
	/// </summary>
	public void Validate()
	{
		if (String.IsNullOrEmpty(Endpoint))
		{
			throw new InvalidOperationException("Není nastaven Cosmos:Endpoint.");
		}

		if (String.IsNullOrEmpty(DatabaseId))
		{
			throw new InvalidOperationException("Není nastaven Cosmos:DatabaseId.");
		}
	}
}
