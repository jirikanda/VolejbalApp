namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial class PrihlaskaDbRepository : IPrihlaskaRepository
{
	public async Task<Prihlaska> GetPrihlaska(int terminId, int osobaId)
	{
		return await Data.Where(item => (item.TerminId == terminId) && (item.OsobaId == osobaId)).SingleOrDefaultAsync();
	}
}
