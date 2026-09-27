using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.Facades.Terminy.Dto.Extensions;
using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

namespace KandaEu.Volejbal.Facades.Terminy;

[Service(ServiceType = typeof(ITerminApi))]
public class TerminFacade(
	ITerminRepository _terminRepository,
	IOsobaRepository _osobaRepository,
	IEnsureTerminyService _ensureTerminyService,
	TimeProvider _timeProvider) : ITerminApi
{
	/// <summary>
	/// Vrátí seznam budoucích termínů. Pokud jich není k dispozici dost, nejprve je doplní.
	/// </summary>
	/// <remarks>
	/// Termíny se zakládají líně při čtení seznamu, dřívější hodinový timer trigger je proto zrušený.
	/// Souběh více uživatelů řeší EnsureTerminyService (id termínu = jeho datum, takže duplicitu
	/// odmítne Cosmos konfliktem + opakování pokusu). Služba seznam čte i vrací sama, takže seznam
	/// termínů stojí jedno čtení, a když se zakládalo, tak pořád jen jedno plus zápisy.
	/// </remarks>
	public async Task<TerminListDto> GetTerminyAsync(CancellationToken cancellationToken)
	{
		List<Termin> terminy = await _ensureTerminyService.EnsureTerminyAsync(cancellationToken);

		return new TerminListDto
		{
			Terminy = terminy
				.Select(termin => new TerminDto
				{
					Id = termin.Id,
					Datum = termin.Datum
				})
				.ToList()
		};
	}

	public async Task<TerminDetailDto> GetDetailTerminuAsync(string terminId, CancellationToken cancellationToken = default)
	{
		Termin termin = await _terminRepository.GetTerminAsync(terminId, cancellationToken);
		termin.ThrowIfDeleted();
		termin.ThrowIfPast(_timeProvider.GetLocalToday());

		// Detail potřebuje i nepřihlášené, takže seznam osob se načítá tak jako tak - jména přihlášených
		// se proto do přihlášek nedenormalizují, spárují se tady.
		List<Osoba> osoby = await _osobaRepository.GetAllAsync(cancellationToken);
		Dictionary<string, Osoba> osobyPodleId = osoby.ToDictionary(osoba => osoba.Id);

		// Přihlášky/odhlášky smazaných osob přeskakujeme - jinak by se smazaná osoba, která se
		// z termínu kdy odhlásila, objevila mezi nepřihlášenými a šla by znovu přihlásit.
		List<Prihlaska> prihlaskyZnamychOsob = termin.Prihlasky
			.Where(prihlaska => osobyPodleId.ContainsKey(prihlaska.OsobaId))
			.ToList();

		List<Prihlaska> prihlasky = prihlaskyZnamychOsob.Where(prihlaska => prihlaska.Deleted == null).ToList();

		// Neaktivní osoby se k přihlášení nenabízejí. Filtrovat je nelze u přihlášených: osoba
		// deaktivovaná poté, co se na termín přihlásila, musí zůstat vidět mezi přihlášenými.
		List<Osoba> odhlaseni = prihlaskyZnamychOsob
			.Where(prihlaska => prihlaska.Deleted != null)
			.Select(prihlaska => osobyPodleId[prihlaska.OsobaId])
			.Where(osoba => osoba.Aktivni)
			.ToList();

		HashSet<string> rozhodnutiIds = prihlaskyZnamychOsob.Select(prihlaska => prihlaska.OsobaId).ToHashSet();

		List<Osoba> neprihlaseni = osoby
			.Where(osoba => osoba.Aktivni)
			.Where(osoba => !rozhodnutiIds.Contains(osoba.Id))
			.ToList();

		return new TerminDetailDto
		{
			Prihlaseni = prihlasky
				.OrderBy(prihlaska => prihlaska.DatumPrihlaseni)
				.Select(prihlaska => new PrihlasenaOsobaDto
				{
					Osoba = osobyPodleId[prihlaska.OsobaId].ToOsobaDto()
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
				.OrderByPrijmeniJmeno(neprihlasenaOsoba => neprihlasenaOsoba.Osoba.PrijmeniJmeno)
				.ToList()
		};
	}
}
