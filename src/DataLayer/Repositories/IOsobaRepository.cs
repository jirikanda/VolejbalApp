namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial interface IOsobaRepository
{
	Task<List<Osoba>> GetAllAktivniAsync(CancellationToken cancellationToken = default);
}
