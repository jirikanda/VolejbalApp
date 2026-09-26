using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.Reporty.Dto;

namespace KandaEu.Volejbal.Facades.Reporty;

[Service(ServiceType = typeof(IReportTerminuApi))]
public class ReportTerminuFacade(
	ITerminRepository _terminRepository,
	ITimeService _timeService) : IReportTerminuApi
{
	public async Task<ReportTerminu> GetReportAsync(CancellationToken cancellationToken)
	{
		DateTime today = _timeService.GetCurrentDate();
		DateTime datumOdInclusive = ReportHelpers.GetZacatekSkolnihoRoku(today);

		List<Termin> terminy = await _terminRepository.GetTerminyVObdobiAsync(datumOdInclusive, today, cancellationToken);

		return new ReportTerminu
		{
			ObsazenostTerminu = terminy
				.Select(termin => new ReportTerminuItem
				{
					Datum = termin.Datum,
					PocetHracu = termin.Prihlasky.Count(prihlaska => prihlaska.Deleted == null)
				})
				.ToList()
		};
	}
}
