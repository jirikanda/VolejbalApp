using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using KandaEu.Volejbal.Contracts.Reporty;
using Microsoft.EntityFrameworkCore;
using Havit.Data.EntityFrameworkCore;

namespace KandaEu.Volejbal.Facades.Reporty;

[Service]
public class ReportTerminuFacade(
	ITerminDataSource _terminDataSource,
	ITimeService _timeService) : IReportTerminuFacade
{
	public async Task<ReportTerminu> GetReportAsync(CancellationToken cancellationToken)
	{
		DateTime today = _timeService.GetCurrentDate();
		DateTime datumOdInclusive = ReportHelpers.GetZacatekSkolnihoRoku(_timeService);

		return new ReportTerminu
		{
			ObsazenostTerminu = await _terminDataSource.Data
				.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetReportAsync)))
				.Where(termin => (termin.Datum >= datumOdInclusive) && (termin.Datum < today))
				.OrderBy(item => item.Datum)
				.Select(termin => new ReportTerminuItem
				{
					Datum = termin.Datum,
					PocetHracu = termin.Prihlasky.Where(prihlaska => prihlaska.Deleted == null).Count()
				})
				.ToListAsync(cancellationToken)
		};
	}
}
