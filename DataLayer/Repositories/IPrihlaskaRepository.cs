namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial interface IPrihlaskaRepository
{
    Task<Prihlaska> GetPrihlaska(int terminId, int osobaId);
}
