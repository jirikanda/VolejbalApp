namespace KandaEu.Volejbal.Api.Mcp;

/// <summary>
/// Výsledek MCP nástroje se seznamem termínů.
/// </summary>
public class McpTerminListDto
{
	public List<McpTerminDto> Terminy { get; set; }
}
