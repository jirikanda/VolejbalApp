namespace KandaEu.Volejbal.Services.SkolniRok;

public interface ISkolniRokService
{
	/// <summary>
	/// Začátek školního roku (1. září), do kterého zadané datum spadá. Reporty (účast hráčů, obsazenost
	/// termínů) se počítají od něj.
	/// </summary>
	DateTime GetZacatek(DateTime datum);
}
