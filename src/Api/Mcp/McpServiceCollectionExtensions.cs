using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;

namespace KandaEu.Volejbal.Api.Mcp;

public static class McpServiceCollectionExtensions
{
	/// <summary>
	/// Instrukce, které MCP klient předá modelu spolu se seznamem nástrojů.
	/// </summary>
	private const string ServerInstructions = "Přihlašování na volejbal. Hráče najdi nástrojem seznam_hracu (podle jména), termíny i s tím, kdo přijde a kdo ne, nástrojem seznam_terminu. Termín se určuje datem (yyyy-MM-dd), hráč svým id. Přihlášení = prihlasit, odhlášení i omluvení předem (hráč nepřijde) = odhlasit. Než někoho přihlásíš nebo odhlásíš, ujisti se, že jde o správného hráče a termín.";

	/// <summary>
	/// Registruje MCP server obsluhovaný triggerem /mcp (Api/Functions/McpFunctions.cs).
	/// </summary>
	public static IServiceCollection AddVolejbalMcpServer(this IServiceCollection services)
	{
		services.AddMcpServer(options =>
			{
				options.ServerInfo = new Implementation
				{
					Name = "VolejbalApp",
					Version = "1.0.0"
				};
				options.ServerInstructions = ServerInstructions;
			})
			.WithHttpTransport(options => options.Stateless = true)
			.WithTools<VolejbalMcpTools>()
			.WithRequestFilters(filters => filters.AddCallToolFilter(McpToolErrorFilter.Create));

		services.AddSingleton<McpRequestHandler>();

		return services;
	}
}
