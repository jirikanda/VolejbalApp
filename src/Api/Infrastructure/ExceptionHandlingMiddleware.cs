using System.Security;
using Havit;
using Havit.Data.Patterns.Exceptions;
using Havit.AspNetCore.ExceptionMonitoring.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Api.Infrastructure;

/// <summary>
/// Převádí výjimky na JSON odpověď se status kódem.
/// </summary>
/// <remarks>
/// Nahrazuje Havit.AspNetCore.Mvc ErrorToJson middleware, který ve Functions použít nelze - ASP.NET Core
/// integrace nezpřístupňuje middleware pipeline. Mapování status kódů i tvar odpovědi jsou zachované,
/// aby se klientovi nezměnil kontrakt.
/// </remarks>
public class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> _logger) : IFunctionsWorkerMiddleware
{
	public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
	{
		try
		{
			await next(context);
		}
		catch (Exception exception) when (!IsRequestAborted(exception, context))
		{
			(int statusCode, bool handled) = MapException(exception);

			if (handled)
			{
				_logger.LogDebug(exception, "Výjimka namapovaná na status kód {StatusCode}.", statusCode);
			}
			else
			{
				_logger.LogError(exception, "Neošetřená výjimka při zpracování funkce {FunctionName}.", context.FunctionDefinition.Name);
				ReportToExceptionMonitoring(context, exception);
			}

			SetErrorResult(context, statusCode, exception);
		}
	}

	private static (int StatusCode, bool Handled) MapException(Exception exception)
	{
		if (exception is SecurityException)
		{
			return (StatusCodes.Status403Forbidden, true);
		}

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
	private static void SetErrorResult(FunctionContext context, int statusCode, Exception exception)
	{
		InvocationResult invocationResult = context.GetInvocationResult();
		invocationResult.Value = new ObjectResult(ValidationErrorModel.FromException(statusCode, exception))
		{
			StatusCode = statusCode
		};
	}
}
