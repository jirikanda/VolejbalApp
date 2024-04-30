using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.Facades.Terminy.Dto.Extensions;

namespace KandaEu.Volejbal.Facades.Terminy;

[Service]
public class TerminFacade(
	ITerminDataSource _terminDataSource,
	IPrihlaskaDataSource _prihlaskaDataSource,
	IOsobaDataSource _osobaDataSource,
	ITimeService _timeService) : ITerminFacade
{
	public async Task<TerminListDto> GetTerminyAsync(CancellationToken cancellationToken)
	{
		var terminy = await _terminDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetTerminyAsync)))
			.Where(termin => termin.Datum.Date >= _timeService.GetCurrentDate())
			.Select(item => new TerminDto
			{
				Id = item.Id,
				Datum = item.Datum
			}).ToListAsync(cancellationToken);

		return new TerminListDto
		{
			Terminy = terminy
		};
	}

	public async Task<TerminDetailDto> GetDetailTerminuAsync(int terminId, CancellationToken cancellationToken = default)
	{
		List<Prihlaska> prihlaskyIncludingDeleted = await _prihlaskaDataSource.DataIncludingDeleted
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetDetailTerminuAsync)))
			.Where(prihlaska => prihlaska.TerminId == terminId)
			.Include(prihlaska => prihlaska.Osoba)
			.ToListAsync(cancellationToken);

		List<Prihlaska> prihlasky = prihlaskyIncludingDeleted.Where(prihlaska => prihlaska.Deleted == null).ToList();
		List<Prihlaska> odhlasky = prihlaskyIncludingDeleted.Where(prihlaska => prihlaska.Deleted != null).ToList();

		List<Osoba> prihlaseni = prihlasky.Select(item => item.Osoba).ToList();
		List<Osoba> odhlaseni = odhlasky.Select(item => item.Osoba).Distinct().ToList();

		List<Osoba> neprihlaseni = (await _osobaDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetDetailTerminuAsync)))
			.Where(osoba => osoba.Aktivni)
			.ToListAsync(cancellationToken))
			.Except(prihlaseni /* in memory */)
			.Except(odhlaseni /* in memory */)
			.ToList();

		return new TerminDetailDto
		{
			Prihlaseni = prihlasky
				.OrderBy(item => item.DatumPrihlaseni)
				.Select(prihlaska => new PrihlasenaOsobaDto
				{
					Osoba = prihlaska.ToOsobaDto()
				})
				.ToList(),

			Neprihlaseni = neprihlaseni
				.Select(neprihlaseny => new NeprihlasenaOsobaDto
				{
					Osoba = neprihlaseny.ToOsobaDto(),
					IsOdhlaseny = false
				})
				.Concat(odhlaseni.Select(odhlaseny => new NeprihlasenaOsobaDto
				{
					Osoba = odhlaseny.ToOsobaDto(),
					IsOdhlaseny = true
				}))
				.OrderBy(neprihlasenaOsoba => neprihlasenaOsoba.Osoba.PrijmeniJmeno)
				.ToList()
		};
	}

}
