using Havit.Data.EntityFrameworkCore;
using Havit.Services.TimeServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Services.DeaktivaceOsob;

[Service]
public class DeaktivaceOsobService(
	ILogger<DeaktivaceOsobService> _logger,
	IOsobaDataSource _osobaDataSource,
	ITerminDataSource _terminDataSource,
	IUnitOfWork _unitOfWork,
	ITimeService _timeService) : IDeaktivaceOsobService
{
	public async Task DeaktivujOsobyAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Zjišťuji osoby k deaktivaci...");
		var osobyKDeaktivaci = await GetOsobyKDeaktivaciAsync(cancellationToken);
		_logger.LogInformation("Zjištěno {count} osob k deaktivaci", osobyKDeaktivaci.Count);

		if (osobyKDeaktivaci.Any())
		{
			osobyKDeaktivaci.ForEach(osoba => osoba.Aktivni = false);
			_logger.LogInformation("Ukládám...");
			await _unitOfWork.CommitAsync(cancellationToken);
			_logger.LogInformation("Uloženo.");
		}
	}

	private async Task<List<Osoba>> GetOsobyKDeaktivaciAsync(CancellationToken cancellationToken)
	{
		DateTime twoMonthsAgo = _timeService.GetCurrentDate().AddMonths(-2);
		IQueryable<Osoba> osobySAktivitou = _terminDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetOsobyKDeaktivaciAsync)))
			.Where(item => item.Datum > twoMonthsAgo)
			.SelectMany(item => item.Prihlasky)
			//.Where(prihlaska => prihlaska.Deleted == null) // pokud se osoba v posledních měsících přihlásila a odhlásila, neuvažujme ji do deaktivace
			.Select(prihlaska => prihlaska.Osoba)
			.Distinct();

		IQueryable<Osoba> aktivniOsoby = _osobaDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetOsobyKDeaktivaciAsync)))
			.Where(item => item.Aktivni);

		// osoby k deaktivaci jsou ty aktivní osoby, které nemají žádnou aktivitu
		return await aktivniOsoby.Except(osobySAktivitou).ToListAsync(cancellationToken);
	}
}
