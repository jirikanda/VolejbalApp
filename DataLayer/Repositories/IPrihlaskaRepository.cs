namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial interface IPrihlaskaRepository
{
	Task<List<Prihlaska>> GetPrihlaskyIncludingDeletedAsync(Termin termin, CancellationToken cancellationToken);
	Task<Prihlaska> GetPrihlaskaAsync(int terminId, int osobaId, CancellationToken cancellationToken = default);
}
