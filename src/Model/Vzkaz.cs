namespace KandaEu.Volejbal.Model;

/// <summary>
/// Vzkaz na nástěnce. Dokument v kontejneru "vzkazy".
/// </summary>
public class Vzkaz
{
	/// <summary>
	/// GUID. Cosmos nemá sekvence, přiděluje ho repozitář při vložení.
	/// </summary>
	public string Id { get; set; }

	public string AutorId { get; set; }

	public DateTime DatumVlozeni { get; set; }

	public string Zprava { get; set; }

	public DateTime? Deleted { get; set; }
}
