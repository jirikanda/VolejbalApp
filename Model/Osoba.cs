using System.ComponentModel.DataAnnotations.Schema;

namespace KandaEu.Volejbal.Model;

public class Osoba
{
	public int Id { get; set; }

	[Required]
	[MaxLength(50)]
	public string Prijmeni { get; set; }

	[Required]
	[MaxLength(50)]
	public string Jmeno { get; set; }

	[Required]
	[MaxLength(50)]
	public string Email { get; set; }

	public DateTime? Deleted { get; set; }

	public bool Aktivni { get; set; } = true;

	public List<Prihlaska> Prihlasky { get; } = new List<Prihlaska>();

	[NotMapped]
	public bool PripominkyPovoleny { get; } = true;

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
			throw new InvalidOperationException("Osoba je smazaná.");
		}
	}

	public void ThrowIfAktivni()
	{
		if (this.Aktivni)
		{
			throw new InvalidOperationException("Osoba je aktivní.");
		}
	}

	public void ThrowIfNotAktivni()
	{
		if (!this.Aktivni)
		{
			throw new InvalidOperationException("Osoba je neaktivní.");
		}
	}
}
