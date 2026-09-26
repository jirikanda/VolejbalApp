namespace KandaEu.Volejbal.Model;

/// <summary>
/// Přihláška osoby na termín. Není samostatným dokumentem - je vnořená v <see cref="Termin.Prihlasky" />.
/// </summary>
/// <remarks>
/// Přihláška nemá život mimo termín, vždy se čte celá množina pro jeden termín a je jich pod sto.
/// Vnořením padá potřeba unikátního indexu nad (TerminId, OsobaId) i potřeba joinu při čtení detailu
/// termínu; souběh řeší ETag celého dokumentu termínu.
/// </remarks>
public class Prihlaska
{
	public string OsobaId { get; set; }

	public DateTime DatumPrihlaseni { get; set; }

	/// <summary>
	/// Vyplněné u odhlášky. Odhlášení se neukládá jako zmizení přihlášky, ale jako tombstone - UI
	/// potřebuje odlišit "ještě se nerozhodl" od "aktivně odmítl" (NeprihlasenaOsobaDto.IsOdhlaseny).
	/// </summary>
	public DateTime? Deleted { get; set; }
}
