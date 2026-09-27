namespace KandaEu.Volejbal.DataLayer.Repositories;

public interface IOsobaRepository
{
	/// <summary>
	/// Osoba podle id, včetně smazané. Neexistuje-li (id chodí z URL, tedy zvenčí), skončí <see cref="ObjectNotFoundException" />.
	/// </summary>
	Task<Osoba> GetOsobaAsync(string osobaId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Osoby podle id, včetně smazaných - pro dohledání autorů či hráčů odkazovaných z jiných dokumentů.
	/// Neexistuje-li kterákoli z nich, skončí <see cref="ObjectNotFoundException" />: odkazy mezi dokumenty
	/// vznikají jen v aplikaci (fasády autora či hráče před uložením odkazu načítají), mazání je soft delete
	/// a import ze SQL přenáší všechny osoby, takže chybějící osoba je porušená integrita dat způsobená
	/// ručním zásahem mimo aplikaci. Netoleruje se záměrně, aby se o ní vědělo hned.
	/// </summary>
	Task<List<Osoba>> GetOsobyAsync(IReadOnlyCollection<string> osobaIds, CancellationToken cancellationToken = default);

	/// <summary>
	/// Všechny nesmazané osoby (aktivní i neaktivní), seřazené podle příjmení a jména.
	/// </summary>
	Task<List<Osoba>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Všechny nesmazané aktivní osoby, seřazené podle příjmení a jména.
	/// </summary>
	Task<List<Osoba>> GetAllAktivniAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Vloží osobu a přidělí jí id (Cosmos nemá sekvence, id je GUID).
	/// </summary>
	Task InsertAsync(Osoba osoba, CancellationToken cancellationToken = default);

	Task UpdateAsync(Osoba osoba, CancellationToken cancellationToken = default);
}
