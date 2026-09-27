using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using KandaEu.Volejbal.Services.SkolniRok;

namespace KandaEu.Volejbal.Facades.Reporty;

[Service(ServiceType = typeof(IReportTerminuApi))]
public class ReportTerminuFacade(
	ITerminRepository _terminRepository,
	ISkolniRokService _skolniRokService,
	TimeProvider _timeProvider) : IReportTerminuApi
{
	public async Task<ReportTerminu> GetReportAsync(CancellationToken cancellationToken)
	{
		DateTime today = _timeProvider.GetLocalToday();
		DateTime datumOdInclusive = _skolniRokService.GetZacatek(today);

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
