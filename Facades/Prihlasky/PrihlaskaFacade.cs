using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Prihlasky;

namespace KandaEu.Volejbal.Facades.Prihlasky;

[Service]
public class PrihlaskaFacade(
	IUnitOfWork _unitOfWork,
	ITimeService _timeService,
	ITerminRepository _terminRepository,
	IOsobaRepository _osobaRepository,
	IPrihlaskaRepository _prihlaskaRepository) : IPrihlaskaFacade
{
	public async Task PrihlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken)
	{
		Termin termin = await _terminRepository.GetObjectAsync(terminId, cancellationToken);
		termin.ThrowIfDeleted();
		termin.ThrowIfPast(_timeService.GetCurrentDate());

		Osoba osoba = await _osobaRepository.GetObjectAsync(osobaId, cancellationToken);
		osoba.ThrowIfDeleted();
		osoba.ThrowIfNotAktivni();

		if (await _prihlaskaRepository.GetPrihlaskaAsync(terminId, osobaId, cancellationToken) != null)
		{
			return;
		}

		Prihlaska prihlaska = new Prihlaska
		{
			TerminId = terminId,
			OsobaId = osobaId,
			DatumPrihlaseni = _timeService.GetCurrentTime(),
		};

		_unitOfWork.AddForInsert(prihlaska);
		try
		{
			await _unitOfWork.CommitAsync(cancellationToken);
		}
		catch (DbUpdateException ex) when (IsPrihlaskaUniqueIndexViolation(ex))
		{
			// Souběžné přihlášení druhým requestem — již existuje, považujeme za úspěch (idempotentní).
		}
	}

	public async Task OdhlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken)
	{
		Termin termin = await _terminRepository.GetObjectAsync(terminId, cancellationToken);
		termin.ThrowIfDeleted();
		termin.ThrowIfPast(_timeService.GetCurrentDate());

		Osoba osoba = await _osobaRepository.GetObjectAsync(osobaId, cancellationToken);
		osoba.ThrowIfDeleted();
		osoba.ThrowIfNotAktivni();

		Prihlaska prihlaska = await _prihlaskaRepository.GetPrihlaskaAsync(terminId, osobaId, cancellationToken);
		if (prihlaska != null)
		{
			_unitOfWork.AddForDelete(prihlaska);
		}
		else
		{
			var now = _timeService.GetCurrentTime();

			_unitOfWork.AddForInsert(new Prihlaska
			{
				TerminId = terminId,
				OsobaId = osobaId,
				DatumPrihlaseni = now,
				Deleted = now
			});
		}
		await _unitOfWork.CommitAsync(cancellationToken);
	}

	private static bool IsPrihlaskaUniqueIndexViolation(DbUpdateException ex)
	{
		// SQL Server: 2627 = unique constraint, 2601 = unique index.
		// Pro in-memory provider DbUpdateException nepadá, takže větev pro testy neřešíme.
		var inner = ex.InnerException;
		if (inner == null)
		{
			return false;
		}
		string message = inner.Message ?? string.Empty;
		return message.Contains("UIDX_Prihlaska_TerminId_OsobaId_Deleted", StringComparison.Ordinal);
	}
}
