namespace KandaEu.Volejbal.Facades.Reporty;

public static class ReportHelpers
{
	/// <summary>
	/// Začátek školního roku (1. září), do kterého zadané datum spadá. Reporty (účast hráčů, obsazenost
	/// termínů) se počítají od něj. Čistá funkce nad předaným datem, aby šla testovat bez ITimeService.
	/// </summary>
	public static DateTime GetZacatekSkolnihoRoku(DateTime today)
	{
		// Leden až srpen patří ke školnímu roku, který začal v předchozím roce.
		int rok = (today.Month < 9) ? (today.Year - 1) : today.Year;
		return new DateTime(rok, 9, 1);
	}
}
