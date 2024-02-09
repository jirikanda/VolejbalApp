using Havit.Data.EntityFrameworkCore;

namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial class PrihlaskaDbRepository : IPrihlaskaRepository
{
	public async Task<Prihlaska> GetPrihlaskaAsync(int terminId, int osobaId, CancellationToken cancellationToken)
	{
		return await Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetPrihlaskaAsync)))
			.Where(item => (item.TerminId == terminId) && (item.OsobaId == osobaId)).SingleOrDefaultAsync(cancellationToken);
	}
}
