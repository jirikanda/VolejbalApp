using System.Security;
using Havit;
using Havit.AspNetCore.ExceptionMonitoring.Services;
using KandaEu.Volejbal.DataLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Api.Infrastructure;

/// <summary>
/// Převádí výjimky na JSON odpověď se status kódem, u MCP nástrojů na textový výsledek nástroje.
/// </summary>
/// <remarks>
/// Nahrazuje Havit.AspNetCore.Mvc ErrorToJson middleware, který ve Functions použít nelze - ASP.NET Core
/// integrace nezpřístupňuje middleware pipeline. Mapování status kódů i tvar odpovědi jsou zachované,
/// aby se klientovi nezměnil kontrakt.
/// </remarks>
public class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> _logger) : IFunctionsWorkerMiddleware
{
	/// <summary>
	/// Text odpovědi na neošetřenou výjimku. Její Message se klientovi neposílá: u cizích výjimek může nést
	/// interní údaje - CosmosException vypisuje endpoint účtu a celou diagnostiku požadavku - a klient
	/// obsah odpovědi zobrazuje uživateli (Error.razor). Podrobnosti patří do logu a Application Insights.
	/// </summary>
	private const string NeosetrenaVyjimkaMessage = "Na serveru došlo k neočekávané chybě.";

	/// <summary>
	/// Typ triggeru MCP nástroje (McpToolTriggerAttribute) v metadatech funkce.
	/// </summary>
	private const string McpToolTriggerBindingType = "mcpToolTrigger";

	/// <summary>
	/// Text výsledku MCP nástroje, jehož argument nešel převést na typ parametru.
	/// </summary>
	private const string NeplatneArgumentyMcpMessage = "Neplatné argumenty nástroje, zkontroluj jejich formát podle popisu nástroje.";

	public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
	{
		try
		{
			await next(context);
		}
		catch (Exception exception) when (!IsRequestAborted(exception, context))
		{
			if ((exception is FunctionInputConverterException) && IsMcpToolInvocation(context))
			{
				// Model poslal argument, který nejde převést na typ parametru (datum, GUID). Je to chyba
				// volajícího, ne aplikace - nehlásí se, model dostane text a volání může opravit.
				_logger.LogDebug(exception, "Neplatné argumenty MCP nástroje.");
				SetErrorResult(context, StatusCodes.Status422UnprocessableEntity, ValidationErrorModel.FromMessage(StatusCodes.Status422UnprocessableEntity, NeplatneArgumentyMcpMessage));
				return;
			}

			(int statusCode, bool handled) = MapException(exception);
			ValidationErrorModel errorModel;

			if (handled)
			{
				// Ošetřené výjimky (OperationFailedException apod.) nesou text určený uživateli.
				_logger.LogDebug(exception, "Výjimka namapovaná na status kód {StatusCode}.", statusCode);
				errorModel = ValidationErrorModel.FromException(statusCode, exception);
			}
			else
			{
				_logger.LogError(exception, "Neošetřená výjimka při zpracování funkce {FunctionName}.", context.FunctionDefinition.Name);
				ReportToExceptionMonitoring(context, exception);
				errorModel = ValidationErrorModel.FromMessage(statusCode, NeosetrenaVyjimkaMessage);
			}

			SetErrorResult(context, statusCode, errorModel);
		}
	}

	private static (int StatusCode, bool Handled) MapException(Exception exception)
	{
		if (exception is SecurityException)
		{
			return (StatusCodes.Status403Forbidden, true);
		}

		// ObjectNotFoundException (neexistující termín/osoba z URL) se hlásí stejně jako OperationFailedException:
		// z pohledu klienta jde o tentýž případ "data se mezitím změnila, načti stránku znovu".
		if ((exception is OperationFailedException) || (exception is ObjectNotFoundException))
		{
			return (StatusCodes.Status422UnprocessableEntity, true);
		}

		return (StatusCodes.Status500InternalServerError, false);
	}

	/// <summary>
	/// Zrušený request (odpojený klient, shutdown) není chyba aplikace - nemá se hlásit ani přepisovat odpověď.
	/// </summary>
	private static bool IsRequestAborted(Exception exception, FunctionContext context)
	{
		return (exception is OperationCanceledException) && context.CancellationToken.IsCancellationRequested;
	}

	private static void ReportToExceptionMonitoring(FunctionContext context, Exception exception)
	{
		IExceptionMonitoringService exceptionMonitoringService = context.InstanceServices.GetService<IExceptionMonitoringService>();
		exceptionMonitoringService?.HandleException(exception);
	}

	/// <summary>
	/// Odpověď se nastavuje přes InvocationResult, ne zápisem do HttpResponse po dokončení funkce -
	/// ten je s ASP.NET Core integrací nespolehlivý (response stream už může být odeslaný).
	/// </summary>
	/// <remarks>
	/// MCP nástroj žádnou HTTP odpověď nemá: návratovou hodnotu funkce MCP extension hostu předá modelu
	/// jako text výsledku nástroje. IActionResult by se tam serializoval i se všemi svými vlastnostmi,
	/// proto nástroj dostane jen text chyby - model si z něj přečte, proč akce neprošla (termín v minulosti,
	/// neaktivní hráč...), a může to sdělit uživateli. Výjimku do hostu nepouštíme: worker ji hostu předá
	/// jen jako selhání funkce a MCP klient by dostal obecnou chybu bez našeho textu.
	/// </remarks>
	private static void SetErrorResult(FunctionContext context, int statusCode, ValidationErrorModel errorModel)
	{
		InvocationResult invocationResult = context.GetInvocationResult();

		if (IsMcpToolInvocation(context))
		{
			invocationResult.Value = "Chyba: " + errorModel.Message;
			return;
		}

		invocationResult.Value = new ObjectResult(errorModel)
		{
			StatusCode = statusCode
		};
	}

	private static bool IsMcpToolInvocation(FunctionContext context)
	{
		return context.FunctionDefinition.InputBindings.Values
			.Any(binding => String.Equals(binding.Type, McpToolTriggerBindingType, StringComparison.OrdinalIgnoreCase));
	}
}
