using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.Reporty.Dto;

namespace KandaEu.Volejbal.Facades.Reporty;

[Service(ServiceType = typeof(IReportOsobApi))]
public class ReportOsobFacade(
	ITerminRepository _terminRepository,
	IOsobaRepository _osobaRepository,
	ITimeService _timeService) : IReportOsobApi
{
	/// <remarks>
	/// Co v SQL dělalo GROUP BY, se tady spočítá v paměti nad termíny jedné sezóny - jsou jich desítky
	/// a přihlášky jsou součástí jejich dokumentů, takže je to jeden dotaz bez joinu.
	/// </remarks>
	public async Task<ReportOsob> GetReportAsync(CancellationToken cancellationToken)
	{
		DateTime today = _timeService.GetCurrentDate();
		DateTime datumOdInclusive = ReportHelpers.GetZacatekSkolnihoRoku(today);

		List<Termin> terminy = await _terminRepository.GetTerminyVObdobiAsync(datumOdInclusive, today, cancellationToken);

		Dictionary<string, int> pocetTerminuPodleOsoby = terminy
			.SelectMany(termin => termin.Prihlasky)
			.Where(prihlaska => prihlaska.Deleted == null)
			.GroupBy(prihlaska => prihlaska.OsobaId)
			.ToDictionary(skupina => skupina.Key, skupina => skupina.Count());

		// Jen osoby, které mají v sezóně přihlášku - ostatní v reportu nefigurují. Načítají se i smazané,
		// ty se ale stejně jako dřív z reportu vynechávají.
		List<Osoba> osoby = await _osobaRepository.GetOsobyAsync(pocetTerminuPodleOsoby.Keys.ToList(), cancellationToken);

		return new ReportOsob
		{
			UcastHracu = osoby
				.Where(osoba => osoba.Deleted == null)
				.OrderByPrijmeniJmeno()
				.Select(osoba => new ReportOsobItem
				{
					PrijmeniJmeno = osoba.PrijmeniJmeno,
					PocetTerminu = pocetTerminuPodleOsoby[osoba.Id]
				})
				.ToList()
		};
	}
}
