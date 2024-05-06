using Havit.Data.EntityFrameworkCore;

namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial class OsobaDbRepository : IOsobaRepository
{
	public async Task<List<Osoba>> GetAllAktivniAsync(CancellationToken cancellationToken = default)
	{
		return await Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetAllAktivniAsync)))
			.Where(osoba => osoba.Aktivni)
			.ToListAsync(cancellationToken);
	}
}
