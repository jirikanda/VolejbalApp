using Havit.Services.TimeServices;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Services.Terminy.PripominkyPrihlaseni;

[Service]
public class PripomenutiPrihlaskyService(
	ILogger<PripomenutiPrihlaskyService> _logger,
	ITimeService _timeService,
	ITerminRepository _terminRepository,
	IOsobaRepository _osobaRepository,
	IPrihlaskaRepository _prihlaskaRepository,
	IPripomenutiPrihlaskySender _pripomenutiPrihlaskySender) : IPripomenutiPrihlaskyService
{
	public async Task SendPripominkyAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Zjišťuji příští termín...");
		Termin termin = await _terminRepository.GetNextTerminStartingTommorowAsync(_timeService.GetCurrentTime(), cancellationToken);
		_logger.LogInformation("Příští termín je {termin}.", termin.Datum.ToShortDateString());

#if !DEBUG
		if (termin.Datum != _timeService.GetCurrentDate().AddDays(1))
		{
			_logger.LogInformation("Termín není zítra, nerozesíláme připomínky.");
			return;
		}
#endif

		_logger.LogInformation("Zjišťuji nepřihlášené/neodhlášené...");
		List<Osoba> neprihlaseniNeodhlaseniList = await GetNeprihlaseniNeohlaseniAsync(termin, cancellationToken);
		_logger.LogInformation("Nalezeno {count} nepřihlášených/neodhlášených.", neprihlaseniNeodhlaseniList.Count);

		List<Osoba> osobyKPripomenutí = neprihlaseniNeodhlaseniList.Where(item => item.PripominkyPovoleny).ToList();
		_logger.LogInformation("Povoleny připomínky má {count} nepřihlášených/neodhlášených.", osobyKPripomenutí.Count);

		foreach (Osoba osoba in osobyKPripomenutí)
		{
			_logger.LogInformation("Posílám připomínku {PrijmeniJmeno}...", osoba.PrijmeniJmeno);
			await _pripomenutiPrihlaskySender.SendPripominkaAsync(osoba, cancellationToken);
			_logger.LogInformation("Připomínka odeslána.");
		}
	}

	private async Task<List<Osoba>> GetNeprihlaseniNeohlaseniAsync(Termin termin, CancellationToken cancellationToken = default)
	{
		List<Osoba> aktivniOsoby = await _osobaRepository.GetAllAktivniAsync(cancellationToken);
		List<Prihlaska> prihlasky = await _prihlaskaRepository.GetPrihlaskyIncludingDeletedAsync(termin, cancellationToken);
		var prihlaseneOdhlaseneOsobyId = prihlasky.Select(prihlaska => prihlaska.OsobaId).Distinct().ToHashSet();
		return aktivniOsoby.Where(osoba => !prihlaseneOdhlaseneOsobyId.Contains(osoba.Id)).ToList();
	}
}