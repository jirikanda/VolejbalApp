using Havit.Data.EntityFrameworkCore;
using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.Facades.Terminy.Dto.Extensions;
using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;
using Microsoft.EntityFrameworkCore;

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
		List<Prihlaska> prihlasky = await _prihlaskaDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetDetailTerminuAsync)))
			.Where(prihlaska => prihlaska.TerminId == terminId)
			.Include(prihlaska => prihlaska.Osoba)
			.OrderBy(prihlaska => prihlaska.DatumPrihlaseni)
			.ToListAsync(cancellationToken);

		List<Osoba> prihlaseni = prihlasky
			.Select(item => item.Osoba)
			.ToList();

		List<Osoba> neprihlaseni = (await _osobaDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetDetailTerminuAsync)))
			.Where(osoba => osoba.Aktivni)
			.ToListAsync(cancellationToken))
			.Except(prihlaseni /* in memory */)
			.OrderBy(item => item.PrijmeniJmeno)
			.ToList();

		return new TerminDetailDto
		{
			Prihlaseni = prihlasky.Select(prihlaska => prihlaska.ToOsobaDto()).ToList(),
			Neprihlaseni = neprihlaseni.Select(osoba => osoba.ToOsobaDto()).ToList()
		};
	}

}
