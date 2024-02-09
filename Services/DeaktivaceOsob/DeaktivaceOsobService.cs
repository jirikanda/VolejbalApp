using Havit.Data.EntityFrameworkCore;
using Havit.Services.TimeServices;
using Microsoft.EntityFrameworkCore;

namespace KandaEu.Volejbal.Services.DeaktivaceOsob;

[Service]
public class DeaktivaceOsobService : IDeaktivaceOsobService
{
	private readonly IOsobaDataSource osobaDataSource;
	private readonly ITerminDataSource terminDataSource;
	private readonly IUnitOfWork unitOfWork;
	private readonly ITimeService timeService;

	public DeaktivaceOsobService(IOsobaDataSource osobaDataSource, ITerminDataSource terminDataSource, IUnitOfWork unitOfWork, ITimeService timeService)
	{
		this.osobaDataSource = osobaDataSource;
		this.terminDataSource = terminDataSource;
		this.unitOfWork = unitOfWork;
		this.timeService = timeService;
	}

	public async Task DeaktivujOsobyAsync(CancellationToken cancellationToken)
	{
		var osobyKDeaktivaci = await GetOsobyKDeaktivaciAsync(cancellationToken);
		if (osobyKDeaktivaci.Any())
		{
			osobyKDeaktivaci.ForEach(osoba => osoba.Aktivni = false);
			await unitOfWork.CommitAsync(cancellationToken);
		}
	}

	private async Task<List<Osoba>> GetOsobyKDeaktivaciAsync(CancellationToken cancellationToken)
	{
		DateTime twoMonthsAgo = timeService.GetCurrentDate().AddMonths(-2);
		IQueryable<Osoba> osobySAktivitou = terminDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetOsobyKDeaktivaciAsync)))
			.Where(item => item.Datum > twoMonthsAgo)
			.SelectMany(item => item.Prihlasky)
			.Where(prihlaska => prihlaska.Deleted == null)
			.Select(prihlaska => prihlaska.Osoba)
			.Distinct();

		IQueryable<Osoba> aktivniOsoby = osobaDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetOsobyKDeaktivaciAsync)))
			.Where(item => item.Aktivni);

		// osoby k deaktivaci jsou ty aktivní osoby, které nemají žádnou aktivitu
		return await aktivniOsoby.Except(osobySAktivitou).ToListAsync(cancellationToken);
	}
}
