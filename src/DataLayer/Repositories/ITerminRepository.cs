namespace KandaEu.Volejbal.DataLayer.Repositories;

public interface ITerminRepository
{
	/// <summary>
	/// Termín podle id (= data, "2026-01-13") i s přihláškami a ETagem. Neexistuje-li, nebo nemá-li id tvar
	/// data (id chodí z URL, tedy zvenčí), skončí <see cref="ObjectNotFoundException" />.
	/// </summary>
	Task<Termin> GetTerminAsync(string terminId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Nesmazané termíny ode dneška dál, seřazené podle data.
	/// </summary>
	Task<List<Termin>> GetBudouciTerminyAsync(DateTime today, CancellationToken cancellationToken = default);

	/// <summary>
	/// Datum posledního termínu, včetně smazaných - na něj navazuje zakládání dalších termínů.
	/// Null, pokud žádný termín neexistuje.
	/// </summary>
	Task<DateTime?> GetPosledniDatumTerminuAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Nesmazané termíny v intervalu &lt;odInclusive, doExclusive), seřazené podle data.
	/// </summary>
	Task<List<Termin>> GetTerminyVObdobiAsync(DateTime odInclusive, DateTime doExclusive, CancellationToken cancellationToken = default);

	/// <summary>
	/// Založí termín; id dokumentu odvodí z data. Vrací false, pokud termín s tímtéž datem už existuje
	/// (Cosmos odmítne duplicitní id v partition konfliktem) - typicky ho právě založil souběžný požadavek
	/// a volající si má načíst aktuální stav. Viz <see cref="Termin.Id" />.
	/// </summary>
	Task<bool> TryCreateTerminAsync(Termin termin, CancellationToken cancellationToken = default);

	/// <summary>
	/// Uloží termín i s přihláškami, pokud se od načtení nezměnil (kontrola ETagu).
	/// Vrací false, pokud mezitím termín změnil někdo jiný - volající si ho má načíst znovu a zopakovat.
	/// </summary>
	Task<bool> TryReplaceTerminAsync(Termin termin, CancellationToken cancellationToken = default);
}
