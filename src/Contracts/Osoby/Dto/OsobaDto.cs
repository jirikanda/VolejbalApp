namespace KandaEu.Volejbal.Contracts.Osoby.Dto;

public class OsobaDto
{
	public string Id { get; set; }
	public string PrijmeniJmeno { get; set; }

	/// <summary>
	/// Indikuje, zda je osoba aktivní (neaktivní osoba se nepřihlašuje na termíny).
	/// </summary>
	public bool Aktivni { get; set; }
}
