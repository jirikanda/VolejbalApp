namespace KandaEu.Volejbal.Api.Mcp;

/// <summary>
/// Termín i s tím, kdo přijde a kdo ne, ve tvaru pro jazykový model.
/// </summary>
/// <remarks>
/// Oproti TerminDetailDto jsou nepřihlášení rozdělení na dva seznamy (omluvení a nerozhodnutí) místo
/// příznaku IsOdhlaseny u každé osoby - model tak rozdíl nepřehlédne. Den v týdnu se posílá hotový,
/// aby ho model nemusel z data dopočítávat (a plést se v tom).
/// </remarks>
public class McpTerminDto
{
	/// <summary>
	/// Datum termínu ve tvaru yyyy-MM-dd (= id termínu). Pod tímto názvem ho nástroje prihlasit a odhlasit
	/// přijímají jako parametr.
	/// </summary>
	public string Datum { get; set; }

	public string DenVTydnu { get; set; }

	public List<McpOsobaDto> Prihlaseni { get; set; }

	/// <summary>
	/// Osoby, které se z termínu odhlásily nebo předem omluvily (aktivně odmítly účast).
	/// </summary>
	public List<McpOsobaDto> Omluveni { get; set; }

	/// <summary>
	/// Aktivní osoby, které se k termínu dosud nevyjádřily.
	/// </summary>
	public List<McpOsobaDto> Nerozhodnuti { get; set; }
}
