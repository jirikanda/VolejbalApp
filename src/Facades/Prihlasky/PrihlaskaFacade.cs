using Havit;
using KandaEu.Volejbal.Contracts.Prihlasky;

namespace KandaEu.Volejbal.Facades.Prihlasky;

[Service(ServiceType = typeof(IPrihlaskaApi))]
public class PrihlaskaFacade(
	TimeProvider _timeProvider,
	ITerminRepository _terminRepository,
	IOsobaRepository _osobaRepository) : IPrihlaskaApi
{
	/// <summary>
	/// Maximální počet pokusů o uložení termínu při souběhu (viz UpravTerminAsync).
	/// </summary>
	private const int MaxPocetPokusu = 10;

	public async Task PrihlasitAsync(string terminId, string osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await _osobaRepository.GetOsobaAsync(osobaId, cancellationToken);
		osoba.ThrowIfDeleted();
		osoba.ThrowIfNotAktivni();

		await UpravTerminAsync(terminId, termin =>
		{
			Prihlaska prihlaska = NajdiPrihlasku(termin, osobaId);

			if ((prihlaska != null) && (prihlaska.Deleted == null))
			{
				// Už je přihlášená, není co měnit - přihlášení je idempotentní.
				return false;
			}

			if (prihlaska != null)
			{
				// Dřívější odhlášku měníme zpět na přihlášku.
				prihlaska.Deleted = null;
				prihlaska.DatumPrihlaseni = _timeProvider.GetLocalDateTime();
			}
			else
			{
				termin.Prihlasky.Add(new Prihlaska
				{
					OsobaId = osobaId,
					DatumPrihlaseni = _timeProvider.GetLocalDateTime()
				});
			}

			return true;
		}, cancellationToken);
	}

	public async Task OdhlasitAsync(string terminId, string osobaId, CancellationToken cancellationToken)
	{
		await UpravTerminAsync(terminId, termin =>
		{
			Prihlaska prihlaska = NajdiPrihlasku(termin, osobaId);

			if ((prihlaska != null) && (prihlaska.Deleted != null))
			{
				// Už je odhlášená, není co měnit.
				return false;
			}

			if (prihlaska != null)
			{
				prihlaska.Deleted = _timeProvider.GetLocalDateTime();
			}
			else
			{
				// Odhlášení osoby, která přihlášená nebyla: tombstone, aby UI poznalo, že jde
				// o aktivní odmítnutí účasti, ne o "ještě se nerozhodl".
				DateTime now = _timeProvider.GetLocalDateTime();
				termin.Prihlasky.Add(new Prihlaska
				{
					OsobaId = osobaId,
					DatumPrihlaseni = now,
					Deleted = now
				});
			}

			return true;
		}, cancellationToken);
	}

	private static Prihlaska NajdiPrihlasku(Termin termin, string osobaId)
	{
		// Na osobu připadá nejvýš jedna položka - přihlášení i odhlášení mění tutéž (viz Prihlaska).
		return termin.Prihlasky.FirstOrDefault(prihlaska => prihlaska.OsobaId == osobaId);
	}

	/// <summary>
	/// Načte termín, nechá ho upravit a uloží. Pokud mezitím termín změnil někdo jiný, načte ho znovu
	/// a úpravu zopakuje nad aktuálními daty.
	/// </summary>
	/// <param name="uprava">Úprava termínu. Vrací false, pokud není co ukládat.</param>
	/// <remarks>
	/// Přihlášky jsou součástí dokumentu termínu, takže souběžná přihlášení dvou hráčů na tentýž termín
	/// píšou do téhož dokumentu. Konflikt zachytí ETag (uložení skončí s 412) - cenou za vnoření
	/// přihlášek je tahle smyčka, výměnou za ni odpadl unikátní index i join při čtení detailu.
	/// </remarks>
	private async Task UpravTerminAsync(string terminId, Func<Termin, bool> uprava, CancellationToken cancellationToken)
	{
		for (int pokus = 1; ; pokus++)
		{
			Termin termin = await _terminRepository.GetTerminAsync(terminId, cancellationToken);
			termin.ThrowIfDeleted();
			termin.ThrowIfPast(_timeProvider.GetLocalToday());

			if (!uprava(termin))
			{
				return;
			}

			if (await _terminRepository.TryReplaceTerminAsync(termin, cancellationToken))
			{
				return;
			}

			if (pokus >= MaxPocetPokusu)
			{
				throw new OperationFailedException("Termín se právě mění z jiného zařízení, zkus to prosím znovu.");
			}
		}
	}
}
