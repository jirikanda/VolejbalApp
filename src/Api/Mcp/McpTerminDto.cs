namespace KandaEu.Volejbal.Api.Mcp;

/// <summary>
/// Termín i s tím, kdo přijde a kdo ne, ve tvaru pro jazykový model.
/// </summary>
/// <remarks>
/// Oproti TerminDetailDto jsou osoby bez přihlášky rozdělené na dva seznamy (odhlášení a nepřihlášení) místo
/// příznaku IsOdhlaseny u každé osoby - model tak rozdíl nepřehlédne.
/// </remarks>
public class McpTerminDto
{
	/// <summary>
	/// Datum termínu. Serializuje se jako yyyy-MM-dd - v tomto tvaru (= id termínu) ho pod stejným názvem
	/// přijímají nástroje prihlasit a odhlasit.
	/// </summary>
	public DateOnly Datum { get; set; }

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
