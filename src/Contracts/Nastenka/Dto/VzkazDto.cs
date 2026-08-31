namespace KandaEu.Volejbal.Contracts.Nastenka.Dto;

public class VzkazDto
{
	public string Author { get; set; }
	public string Zprava { get; set; }
	public DateTime DatumVlozeni { get; set; }

	/// <summary>
	/// True, pokud vzkaz pochází ze starší "vlny" než aktuální týden (vlna začíná každou středu).
	/// UI zobrazuje obsolete vzkazy decentně (šedý pruh) a aktuální výrazně (modrý pruh).
	/// </summary>
	public bool IsObsolete { get; set; }
}
