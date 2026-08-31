namespace KandaEu.Volejbal.Contracts.Osoby.Dto;

public class NeprihlasenaOsobaDto
{
	public OsobaDto Osoba { get; set; }

	/// <summary>
	/// Indikuje, zda již byla osoba přihlášena a následně se odhlásila, tedy zda proběhl akt aktivního odmítnutí účasti na termínu.
	/// </summary>
	public bool IsOdhlaseny { get; set; }
}
