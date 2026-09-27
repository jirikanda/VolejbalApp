using Havit;

namespace KandaEu.Volejbal.Model;

/// <summary>
/// Osoba (hráč). Dokument v kontejneru "osoby".
/// </summary>
public class Osoba
{
	/// <summary>
	/// GUID. Cosmos nemá sekvence, přiděluje ho repozitář při vložení.
	/// </summary>
	public string Id { get; set; }

	public string Prijmeni { get; set; }

	public string Jmeno { get; set; }

	public string Email { get; set; }

	public DateTime? Deleted { get; set; }

	public bool Aktivni { get; set; } = true;

	[JsonIgnore]
	public string PrijmeniJmeno
	{
		get
		{
			return (this.Prijmeni + " " + this.Jmeno).Trim();
		}
	}

	public void ThrowIfDeleted()
	{
		if (this.Deleted != null)
		{
			throw new OperationFailedException("Osoba je smazaná.");
		}
	}

	public void ThrowIfAktivni()
	{
		if (this.Aktivni)
		{
			throw new OperationFailedException("Osoba je aktivní.");
		}
	}

	public void ThrowIfNotAktivni()
	{
		if (!this.Aktivni)
		{
			throw new OperationFailedException("Osoba je neaktivní.");
		}
	}
}
