namespace KandaEu.Volejbal.Contracts.Api;

/// <summary>
/// Cesty HTTP API. Konstanty sdílí Refit kontrakty (I*Api) s HTTP triggery v projektu Api,
/// takže se obě strany nemohou rozejít. Proto také host.json vyprazdňuje routePrefix - cesty
/// jsou tu uvedené včetně "api/".
/// </summary>
/// <remarks>
/// Bez route constraints (např. "{osobaId:int}"): Refit constraints v šabloně neumí a konstanta
/// je sdílená. Odpovídá to dosavadnímu stavu, kdy je controllery také neměly.
/// </remarks>
public static class ApiRoutes
{
	public const string Nastenka = "api/nastenka";

	public const string Osoby = "api/osoby";
	public const string OsobyAktivni = "api/osoby/aktivni";
	public const string Osoba = "api/osoby/{osobaId}";
	public const string OsobaAktivovat = "api/osoby/{osobaId}/aktivovat";
	public const string OsobaDeaktivovat = "api/osoby/{osobaId}/deaktivovat";

	public const string Terminy = "api/terminy";
	public const string Termin = "api/terminy/{terminId}";
	public const string TerminPrihlasit = "api/terminy/{terminId}/osoby/{osobaId}/prihlasit";
	public const string TerminOdhlasit = "api/terminy/{terminId}/osoby/{osobaId}/odhlasit";

	public const string ReportyTerminy = "api/reporty/terminy";
	public const string ReportyOsoby = "api/reporty/osoby";

	public const string SystemSeed = "api/system/seed/{profileName}";

	public const string Health = "api/health";
}
