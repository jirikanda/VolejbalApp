namespace KandaEu.Volejbal.Api.Mcp;

/// <summary>
/// Osoba v detailu termínu pro MCP nástroj.
/// </summary>
/// <remarks>
/// Vlastní typ místo OsobaDto: TerminFacade v detailu termínu příznak Aktivni nevyplňuje (detail ho
/// nepotřebuje), takže serializované OsobaDto by modelu tvrdilo, že je každý neaktivní.
/// </remarks>
public class McpOsobaDto
{
	public string Id { get; set; }
	public string PrijmeniJmeno { get; set; }
}
