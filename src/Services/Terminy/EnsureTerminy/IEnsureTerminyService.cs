namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

public interface IEnsureTerminyService
{
	/// <summary>
	/// Doplní budoucí termíny do požadovaného počtu a vrátí nesmazané budoucí termíny (včetně právě
	/// založených), seřazené podle data. Volající si tedy seznam nemusí číst znovu.
	/// </summary>
	Task<List<Termin>> EnsureTerminyAsync(CancellationToken cancellationToken);
}
