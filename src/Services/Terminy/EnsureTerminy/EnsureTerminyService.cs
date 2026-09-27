using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

[Service]
public class EnsureTerminyService(
	ILogger<EnsureTerminyService> _logger,
	ITerminRepository _terminRepository,
	ITerminDatumGeneratorService _terminDatumGeneratorService,
	TimeProvider _timeProvider) : IEnsureTerminyService
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
	/// Doplní budoucí termíny do počtu <see cref="PozadovanyPocetBudoucichTerminu" /> a vrátí nesmazané
	/// budoucí termíny včetně právě založených.
	/// </summary>
	/// <remarks>
	/// Metoda se volá při čtení seznamu termínů, může tedy běžet souběžně pro několik požadavků najednou.
	/// Souběh řeší databáze: id dokumentu termínu je jeho datum, takže Cosmos duplicitní termín odmítne
	/// konfliktem a <see cref="ITerminRepository.TryCreateTerminAsync" /> vrátí false. Poražený v závodě to
	/// zkusí znovu nad aktuálními daty - obvykle už nezbývá co zakládat.
	///
	/// Aby souběžné běhy soupeřily o tentýž dokument, musí počet budoucích termínů i datum, na které se
	/// navazuje, pocházet z jednoho snímku dat. Proto se čte jediný seznam budoucích termínů včetně
	/// smazaných a poslední datum se z něj bere přímo; samostatný dotaz na poslední datum přijde na řadu
	/// jen tehdy, když žádný budoucí termín není. Dva dotazy s různým stářím by dovolily, aby poražený
	/// navázal na datum založené soupeřem a místo konfliktu založil termín navíc.
	/// </remarks>
	public async Task<List<Termin>> EnsureTerminyAsync(CancellationToken cancellationToken)
	{
		DateTime today = _timeProvider.GetLocalToday();

		for (int pokus = 1; pokus <= MaxPocetPokusu; pokus++)
		{
			(bool uspech, List<Termin> budouciTerminy) = await TryEnsureTerminyCoreAsync(today, cancellationToken);
			if (uspech)
			{
				return budouciTerminy;
			}

			_logger.LogInformation("Založení termínu selhalo na konfliktu (pokus {pokus}), pravděpodobně souběh s jiným požadavkem. Zkouším znovu.", pokus);
		}

		// Po vyčerpání pokusů termíny nejspíš založil někdo jiný; vrací se to, co je k dispozici, další
		// požadavek to dorovná.
		_logger.LogWarning("Termíny se nepodařilo doplnit ani na {pokusy}. pokus, nechávám to na dalším požadavku.", MaxPocetPokusu);
		return await _terminRepository.GetBudouciTerminyAsync(today, cancellationToken);
	}

	/// <summary>
	/// Jeden průchod doplněním termínů. Vrací false, pokud narazil na konflikt nebo na data změněná
	/// souběžným požadavkem; při úspěchu vrací nesmazané budoucí termíny včetně založených.
	/// </summary>
	private async Task<(bool Uspech, List<Termin> BudouciTerminy)> TryEnsureTerminyCoreAsync(DateTime today, CancellationToken cancellationToken)
	{
		// Jeden snímek: nesmazané termíny určují počet, všechny (i smazané) datum, na které se navazuje.
		List<Termin> vsechnyBudouciTerminy = await _terminRepository.GetBudouciTerminyIncludingDeletedAsync(today, cancellationToken);
		List<Termin> budouciTerminy = vsechnyBudouciTerminy.Where(termin => termin.Deleted == null).ToList();
		_logger.LogInformation("Nalezeno {count} budoucích termínů.", budouciTerminy.Count);

		if (budouciTerminy.Count >= PozadovanyPocetBudoucichTerminu)
		{
			return (true, budouciTerminy);
		}

		DateTime? posledniDatum;
		if (vsechnyBudouciTerminy.Count > 0)
		{
			posledniDatum = vsechnyBudouciTerminy.Max(termin => termin.Datum);
		}
		else
		{
			posledniDatum = await _terminRepository.GetPosledniDatumTerminuAsync(cancellationToken);
			if ((posledniDatum != null) && (posledniDatum.Value >= today))
			{
				// Mezi oběma dotazy někdo budoucí termín založil - snímek už neplatí, zkusíme to znovu.
				return (false, budouciTerminy);
			}
		}

		foreach (DateTime datum in _terminDatumGeneratorService.GetDatumyKZalozeni(today, posledniDatum, budouciTerminy.Count, PozadovanyPocetBudoucichTerminu))
		{
			_logger.LogInformation("Zakládám termín pro datum {datum}.", datum);
			Termin termin = new Termin { Datum = datum };
			if (!await _terminRepository.TryCreateTerminAsync(termin, cancellationToken))
			{
				return (false, budouciTerminy);
			}

			budouciTerminy.Add(termin);
		}

		return (true, budouciTerminy.OrderBy(termin => termin.Datum).ToList());
	}
}
