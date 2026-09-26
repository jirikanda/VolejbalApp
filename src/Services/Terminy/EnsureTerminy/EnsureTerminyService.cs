using Havit.Services.TimeServices;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

[Service]
public class EnsureTerminyService(
	ILogger<EnsureTerminyService> _logger,
	ITerminRepository _terminRepository,
	ITerminDatumGeneratorService _terminDatumGeneratorService,
	ITimeService _timeService) : IEnsureTerminyService
{
	/// <summary>
	/// Kolik budoucích termínů má být k dispozici.
	/// </summary>
	public const int PozadovanyPocetBudoucichTerminu = 3;

	/// <summary>
	/// Maximální počet pokusů o založení termínů (kvůli souběhu, viz EnsureTerminyAsync).
	/// </summary>
	private const int MaxPocetPokusu = 3;

	/// <summary>
	/// Doplní budoucí termíny do počtu <see cref="PozadovanyPocetBudoucichTerminu" />.
	/// </summary>
	/// <remarks>
	/// Metoda se volá při čtení seznamu termínů, může tedy běžet souběžně pro několik požadavků najednou.
	/// Souběh řeší databáze: id dokumentu termínu je jeho datum, takže Cosmos duplicitní termín odmítne
	/// konfliktem a <see cref="ITerminRepository.TryCreateTerminAsync" /> vrátí false. Poražený v závodě to
	/// zkusí znovu nad aktuálními daty - obvykle už nezbývá co zakládat. Zamykat nic nepotřebujeme,
	/// vkládané datum je deterministické (stejný den v týdnu), takže souběžné běhy soupeří o tentýž dokument.
	/// </remarks>
	public async Task EnsureTerminyAsync(CancellationToken cancellationToken)
	{
		for (int pokus = 1; pokus <= MaxPocetPokusu; pokus++)
		{
			if (await TryEnsureTerminyCoreAsync(cancellationToken))
			{
				return;
			}

			_logger.LogInformation("Založení termínu selhalo na konfliktu (pokus {pokus}), pravděpodobně souběh s jiným požadavkem. Zkouším znovu.", pokus);
		}

		// Po vyčerpání pokusů termíny nejspíš založil někdo jiný; volající si seznam čte znovu tak jako tak.
		_logger.LogWarning("Termíny se nepodařilo doplnit ani na {pokusy}. pokus, nechávám to na dalším požadavku.", MaxPocetPokusu);
	}

	/// <summary>
	/// Jeden průchod doplněním termínů. Vrací false, pokud narazil na konflikt (termín už existuje).
	/// </summary>
	private async Task<bool> TryEnsureTerminyCoreAsync(CancellationToken cancellationToken)
	{
		DateTime today = _timeService.GetCurrentDate();

		_logger.LogInformation("Zjišťuji počet budoucích termínů...");
		List<Termin> budouciTerminy = await _terminRepository.GetBudouciTerminyAsync(today, cancellationToken);
		_logger.LogInformation("Nalezeno {count} budoucích termínů.", budouciTerminy.Count);

		if (budouciTerminy.Count >= PozadovanyPocetBudoucichTerminu)
		{
			return true;
		}

		DateTime? posledniDatum = await _terminRepository.GetPosledniDatumTerminuAsync(cancellationToken);

		foreach (DateTime datum in _terminDatumGeneratorService.GetDatumyKZalozeni(today, posledniDatum, budouciTerminy.Count, PozadovanyPocetBudoucichTerminu))
		{
			_logger.LogInformation("Zakládám termín pro datum {datum}.", datum);
			if (!await _terminRepository.TryCreateTerminAsync(new Termin { Datum = datum }, cancellationToken))
			{
				return false;
			}
		}

		return true;
	}
}
