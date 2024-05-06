using Havit.Data.EntityFrameworkCore;

namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial class TerminDbRepository : ITerminRepository
{
	public async Task<Termin> GetNextTerminStartingTommorowAsync(DateTime now, CancellationToken cancellationToken = default)
	{
		DateTime tomorow = now.Date.AddDays(1);

		return await Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetNextTerminStartingTommorowAsync)))
			.OrderBy(item => item.Datum)
			.Where(item => item.Datum >= tomorow)
			.FirstAsync(cancellationToken);
	}

}
