namespace KandaEu.Volejbal.Api.Mcp;

/// <summary>
/// Termín i s tím, kdo přijde a kdo ne, ve tvaru pro jazykový model.
/// </summary>
/// <remarks>
/// Oproti TerminDetailDto jsou osoby bez přihlášky rozdělené na dva seznamy (odhlášení a nepřihlášení) místo
/// příznaku IsOdhlaseny u každé osoby - model tak rozdíl nepřehlédne. Den v týdnu se posílá hotový,
/// aby ho model nemusel z data dopočítávat (a plést se v tom).
/// </remarks>
public class McpTerminDto
{
	/// <summary>
	/// Datum termínu. Serializuje se jako yyyy-MM-dd - v tomto tvaru (= id termínu) ho pod stejným názvem
	/// přijímají nástroje prihlasit a odhlasit.
	/// </summary>
	public DateOnly Datum { get; set; }

	/// <summary>
	/// Den v týdnu česky ("úterý"), aby model při potvrzení ("přihlášen na úterý 17. 11.") nemusel den
	/// dopočítávat z data - v tom se jazykové modely pletou.
	/// </summary>
	public string DenVTydnu { get; set; }

	public List<McpOsobaDto> Prihlaseni { get; set; }

	/// <summary>
	/// Osoby, které se z termínu odhlásily nebo předem omluvily (aktivně odmítly účast).
	/// </summary>
	public List<McpOsobaDto> Odhlaseni { get; set; }

	/// <summary>
	/// Aktivní osoby, které se k termínu dosud nevyjádřily.
	/// </summary>
	public List<McpOsobaDto> Neprihlaseni { get; set; }
}
