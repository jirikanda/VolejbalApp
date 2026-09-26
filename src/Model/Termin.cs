using System.Globalization;
using Havit;

namespace KandaEu.Volejbal.Model;

/// <summary>
/// Termín (jedno hraní) i se všemi přihláškami. Dokument v kontejneru "terminy".
/// </summary>
public class Termin
{
	/// <summary>
	/// Id termínu je jeho datum ("2026-01-13") a zároveň partition key kontejneru (/id): každý termín je
	/// vlastní logickou partition. Cosmos garantuje unikátnost id v rámci partition, takže je tím zdarma
	/// vynucená unikátnost data - to, co v SQL dělal index UIDX_Termin_Datum_Deleted. Souběžné založení
	/// téhož termínu skončí konfliktem (409), který zopakuje EnsureTerminyService.
	/// </summary>
	/// <remarks>
	/// Partition key odvozený z data (např. sezóna) by nic nepřinesl: seznamy termínů se čtou napříč
	/// sezónami tak jako tak, a kdyby se pravidlo odvození kdy změnilo, staré dokumenty by point read
	/// přestal nacházet. Id je jediná hodnota, kterou má volající vždy v ruce.
	/// </remarks>
	public string Id { get; set; }

	public DateTime Datum { get; set; }

	public DateTime? Deleted { get; set; }

	public List<Prihlaska> Prihlasky { get; set; } = new List<Prihlaska>();

	/// <summary>
	/// ETag dokumentu pro optimistickou konkurenci. Naplňuje ho repozitář z odpovědi Cosmosu, do
	/// dokumentu se neserializuje - systémové vlastnosti si Cosmos spravuje sám.
	/// </summary>
	[JsonIgnore]
	public string ETag { get; set; }

	/// <summary>
	/// Id dokumentu pro dané datum.
	/// </summary>
	public static string GetId(DateTime datum)
	{
		return datum.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Datum termínu z jeho id. Vrací null, pokud id nemá očekávaný tvar - id chodí z URL, tedy zvenčí.
	/// </summary>
	public static DateTime? TryGetDatum(string id)
	{
		if (DateTime.TryParseExact(id, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datum))
		{
			return datum;
		}

		return null;
	}

	public void ThrowIfDeleted()
	{
		if (Deleted != null)
		{
			throw new OperationFailedException("Termín je smazaný.");
		}
	}

	public void ThrowIfPast(DateTime today)
	{
		if (Datum < today.Date)
		{
			throw new OperationFailedException("Termín je v minulosti.");
		}
	}
}
