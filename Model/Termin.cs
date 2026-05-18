using Havit;

namespace KandaEu.Volejbal.Model;

public class Termin
{
	public int Id { get; set; }

	public DateTime Datum { get; set; }

	public DateTime? Deleted { get; set; }

	public List<Prihlaska> Prihlasky { get; } = new List<Prihlaska>();

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
