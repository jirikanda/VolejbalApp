namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial interface IPrihlaskaRepository
{
	Task<Prihlaska> GetPrihlaskaAsync(int terminId, int osobaId, CancellationToken cancellationToken = default);
}
