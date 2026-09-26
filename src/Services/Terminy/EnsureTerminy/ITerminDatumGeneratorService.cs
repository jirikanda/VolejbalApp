namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

public interface ITerminDatumGeneratorService
{
	/// <summary>
	/// Data termínů, které je potřeba založit, aby bylo k dispozici <paramref name="pozadovanyPocet" />
	/// budoucích termínů. Deterministické: pro tytéž vstupy vrací tatáž data, na čemž stojí řešení souběhu
	/// v <see cref="EnsureTerminyService" />.
	/// </summary>
	/// <param name="today">Dnešní datum.</param>
	/// <param name="posledniDatum">Datum posledního existujícího termínu (včetně smazaných), null pokud žádný není.</param>
	/// <param name="pocetBudoucichTerminu">Kolik budoucích termínů už existuje.</param>
	/// <param name="pozadovanyPocet">Kolik budoucích termínů má být k dispozici.</param>
	List<DateTime> GetDatumyKZalozeni(DateTime today, DateTime? posledniDatum, int pocetBudoucichTerminu, int pozadovanyPocet);
}
