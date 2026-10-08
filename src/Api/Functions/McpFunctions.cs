using KandaEu.Volejbal.Api.Mcp;
using KandaEu.Volejbal.Contracts.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace KandaEu.Volejbal.Api.Functions;

/// <summary>
/// MCP server (Model Context Protocol) pro AI asistenty na adrese /mcp. Nástroje jsou ve <see cref="VolejbalMcpTools" />.
/// </summary>
/// <remarks>
/// Protokol obsluhuje MCP SDK (<see cref="McpRequestHandler" />), trigger mu jen předá request. Odpověď SDK
/// zapisuje přímo do HttpResponse, proto funkce vrací <see cref="EmptyResult" />.
/// AuthorizationLevel.Function: na rozdíl od REST API vyžaduje klíč (x-functions-key nebo ?code=) - MCP
/// endpoint je snadno objevitelný a nástroje umí přihlásit kohokoli, viz infra/README.md, sekce MCP server.
/// </remarks>
public class McpFunctions(McpRequestHandler _mcpRequestHandler)
{
	[Function(nameof(McpAsync))]
	public async Task<IActionResult> McpAsync(
		[HttpTrigger(AuthorizationLevel.Function, "get", "post", "delete", Route = ApiRoutes.Mcp)] HttpRequest request,
		FunctionContext functionContext)
	{
		await _mcpRequestHandler.HandleAsync(request.HttpContext, functionContext.InstanceServices);
		return new EmptyResult();
	}
}
