using System.Globalization;

namespace KandaEu.Volejbal.DataLayer;

/// <summary>
/// Řazení hráčů podle jména. Jediné místo, kde je pravidlo zapsané - používá ho repozitář, fasády i export.
/// </summary>
/// <remarks>
/// Cosmos DB řadí řetězce ordinálně (podle kódů znaků), takže ORDER BY nad jménem dá diakritiku až za
/// písmeno Z. Jména se proto řadí v paměti českým comparerem (Čapek před Zemanem, ch za h), ne v dotazu.
/// </remarks>
public static class OsobaRazeniExtensions
{
	private static readonly StringComparer s_czechComparer = StringComparer.Create(CultureInfo.GetCultureInfo("cs-CZ"), ignoreCase: false);

	/// <summary>
	/// Seřadí osoby podle příjmení a jména.
	/// </summary>
	public static IOrderedEnumerable<Osoba> OrderByPrijmeniJmeno(this IEnumerable<Osoba> osoby)
	{
		return osoby
			.OrderBy(osoba => osoba.Prijmeni, s_czechComparer)
			.ThenBy(osoba => osoba.Jmeno, s_czechComparer);
	}

	/// <summary>
	/// Seřadí položky podle zadaného jména ("Příjmení Jméno") - pro DTO, která nesou jen složené jméno.
	/// </summary>
	public static IOrderedEnumerable<T> OrderByPrijmeniJmeno<T>(this IEnumerable<T> items, Func<T, string> prijmeniJmeno)
	{
		return items.OrderBy(prijmeniJmeno, s_czechComparer);
	}
}
