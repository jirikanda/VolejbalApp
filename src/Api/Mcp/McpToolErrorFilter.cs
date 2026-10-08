using System.Security;
using System.Text.Json;
using Havit;
using Havit.AspNetCore.ExceptionMonitoring.Services;
using KandaEu.Volejbal.DataLayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace KandaEu.Volejbal.Api.Mcp;

/// <summary>
/// Převádí výjimky z MCP nástrojů na chybový výsledek nástroje (<c>isError: true</c>) - protějšek
/// ExceptionHandlingMiddleware pro HTTP triggery, se stejným dělením na ošetřené a neošetřené výjimky.
/// Předem kontroluje argumenty podle vstupního schématu nástroje.
/// </summary>
/// <remarks>
/// Bez filtru by SDK z každé výjimky kromě McpException udělalo jen "An error occurred invoking 'prihlasit'."
/// a model by se nedozvěděl, proč akce neprošla. Text ošetřených výjimek je určený uživateli, takže ho
/// model dostane celý; u ostatních (Cosmos apod.) jen obecný text - jejich Message může nést interní
/// údaje - a výjimka jde do logu a exception monitoringu.
/// </remarks>
public static class McpToolErrorFilter
{
	/// <summary>
	/// Text výsledku nástroje při neošetřené výjimce. Shodný s odpovědí HTTP triggerů na 500.
	/// </summary>
	private const string NeosetrenaVyjimkaMessage = "Na serveru došlo k neočekávané chybě.";

	public static McpRequestHandler<CallToolRequestParams, CallToolResult> Create(McpRequestHandler<CallToolRequestParams, CallToolResult> next)
	{
		return async (request, cancellationToken) =>
		{
			string chybaArgumentu = ValidateArguments(request);
			if (chybaArgumentu != null)
			{
				return CreateErrorResult(chybaArgumentu);
			}

			try
			{
				return await next(request, cancellationToken);
			}
			catch (Exception exception) when ((exception is OperationFailedException) || (exception is ObjectNotFoundException) || (exception is SecurityException))
			{
				// Ošetřené výjimky (neaktivní hráč, termín v minulosti, neexistující id...) nesou text pro uživatele.
				return CreateErrorResult(exception.Message);
			}
			catch (Exception exception) when (!IsPropagated(exception, cancellationToken))
			{
				ILogger logger = request.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(McpToolErrorFilter));
				logger.LogError(exception, "Neošetřená výjimka v MCP nástroji {ToolName}.", request.Params?.Name);
				request.Services.GetService<IExceptionMonitoringService>()?.HandleException(exception);

				return CreateErrorResult(NeosetrenaVyjimkaMessage);
			}
		};
	}

	/// <summary>
	/// Kontrola argumentů podle vstupního schématu nástroje: povinné jsou přítomné a textové parametry
	/// dostaly text. Vrací text chyby pro model, nebo null.
	/// </summary>
	/// <remarks>
	/// Chybu argumentu by jinak ohlásilo až SDK při bindování na parametry metody - chybějící jako
	/// ArgumentException, špatný typ jako JsonException. Podle typu výjimky ji ale nejde spolehlivě odlišit
	/// od chyby aplikace (JsonException může přijít i z Cosmosu), a chyba volajícího se nemá hlásit do
	/// exception monitoringu. Proto se kontroluje předem: k nástroji se pak dostanou jen platné argumenty
	/// a každá výjimka z něj je skutečně chyba aplikace. Kontrolují se jen typy, které nástroje používají
	/// (všechny parametry jsou string); formát hodnot (datum, id) hlídají fasády.
	/// </remarks>
	private static string ValidateArguments(RequestContext<CallToolRequestParams> request)
	{
		if (request.MatchedPrimitive is not McpServerTool tool)
		{
			// Neznámý nástroj - odpoví SDK chybou protokolu.
			return null;
		}

		JsonElement schema = tool.ProtocolTool.InputSchema;
		IDictionary<string, JsonElement> arguments = request.Params?.Arguments ?? new Dictionary<string, JsonElement>();

		if (schema.TryGetProperty("required", out JsonElement required))
		{
			foreach (JsonElement requiredProperty in required.EnumerateArray())
			{
				string name = requiredProperty.GetString();
				if (!arguments.TryGetValue(name, out JsonElement value) || (value.ValueKind == JsonValueKind.Null))
				{
					return $"Chybí povinný argument '{name}'.";
				}
			}
		}

		if (schema.TryGetProperty("properties", out JsonElement properties))
		{
			foreach (KeyValuePair<string, JsonElement> argument in arguments)
			{
				if (properties.TryGetProperty(argument.Key, out JsonElement property)
					&& property.TryGetProperty("type", out JsonElement type)
					&& (type.GetString() == "string")
					&& (argument.Value.ValueKind != JsonValueKind.String))
				{
					return $"Argument '{argument.Key}' musí být text.";
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Výjimky, které filtr nechává projít: zrušený request (odpojený klient) a chyby protokolu MCP -
	/// ty SDK samo převádí na odpověď s textem pro model.
	/// </summary>
	private static bool IsPropagated(Exception exception, CancellationToken cancellationToken)
	{
		return ((exception is OperationCanceledException) && cancellationToken.IsCancellationRequested)
			|| (exception is McpException);
	}

	private static CallToolResult CreateErrorResult(string message)
	{
		return new CallToolResult
		{
			IsError = true,
			Content = [new TextContentBlock { Text = message }]
		};
	}
}
