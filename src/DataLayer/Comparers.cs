using System.Globalization;

namespace KandaEu.Volejbal.DataLayer;

public static class Comparers
{
	/// <summary>
	/// České řazení textů (Čapek před Zemanem, ch za h).
	/// </summary>
	/// <remarks>
	/// Cosmos DB řadí řetězce ordinálně (podle kódů znaků), takže ORDER BY nad jménem dá diakritiku
	/// až za písmeno Z. Jména se proto řadí v paměti tímto comparerem, ne v dotazu.
	/// </remarks>
	public static readonly StringComparer CzechComparer = StringComparer.Create(CultureInfo.GetCultureInfo("cs-CZ"), ignoreCase: false);
}
