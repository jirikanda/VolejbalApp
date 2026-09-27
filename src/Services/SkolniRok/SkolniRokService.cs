namespace KandaEu.Volejbal.Services.SkolniRok;

/// <summary>
/// Hranice školního roku. Bez závislostí a s datem předaným parametrem, aby šla testovat bez DI.
/// </summary>
[Service]
public class SkolniRokService : ISkolniRokService
{
	private const int PrvniMesic = 9;

	public DateTime GetZacatek(DateTime datum)
	{
		// Leden až srpen patří ke školnímu roku, který začal v předchozím roce.
		int rok = (datum.Month < PrvniMesic) ? (datum.Year - 1) : datum.Year;
		return new DateTime(rok, PrvniMesic, 1);
	}
}
