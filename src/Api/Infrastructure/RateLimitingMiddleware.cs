using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Api.Infrastructure;

/// <summary>
/// Omezuje počet requestů od jednoho klienta (podle IP adresy, viz ClientPartitionKeyResolver).
/// </summary>
/// <remarks>
/// Nahrazuje AddRateLimiter/UseRateLimiter z původního ASP.NET Core hostingu - ASP.NET Core integrace
/// ve Functions middleware pipeline nezpřístupňuje. Stav čítačů je v paměti instance; protože
/// maximumInstanceCount v infra/main.bicep je 1, je limit fakticky globální (rozpadne se jen při
/// výměně instance, kdy čítače začínají znovu).
/// Request se odmítne dřív, než se zavolá funkce - chrání tedy databázi a data, ne účet za spuštění
/// funkcí (to omezuje maximumInstanceCount).
/// Když se adresu klienta zjistit nepodaří, request se propustí: omylem sdílený čítač pro všechny by
/// aplikaci zablokoval celou.
/// </remarks>
public class RateLimitingMiddleware(ILogger<RateLimitingMiddleware> _logger) : IFunctionsWorkerMiddleware
{
	private const string TooManyRequestsMessage = "Příliš mnoho požadavků. Zkuste to prosím za chvíli znovu.";

	/// <summary>
	/// Token bucket: dávka až 30 requestů naráz (načtení stránky volá API několikrát), trvale 1 request
	/// za sekundu. Běžnému uživateli ani několika hráčům za jednou NAT adresou to nevadí.
	/// </summary>
	private static readonly PartitionedRateLimiter<string> s_limiter = PartitionedRateLimiter.Create<string, string>(partitionKey =>
		(partitionKey == null)
			? RateLimitPartition.GetNoLimiter<string>(null)
			: RateLimitPartition.GetTokenBucketLimiter(partitionKey, _ => new TokenBucketRateLimiterOptions
			{
				TokenLimit = 30,
				TokensPerPeriod = 1,
				ReplenishmentPeriod = TimeSpan.FromSeconds(1),
				QueueLimit = 0
			}));

	public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
	{
		HttpContext httpContext = context.GetHttpContext();
		if (httpContext == null)
		{
			await next(context);
			return;
		}

		string partitionKey = ClientPartitionKeyResolver.GetPartitionKey(httpContext.Request.Headers["X-Forwarded-For"], httpContext.Connection.RemoteIpAddress);

		using RateLimitLease lease = s_limiter.AttemptAcquire(partitionKey);
		if (lease.IsAcquired)
		{
			await next(context);
			return;
		}

		// Jen Debug: při zahlcení by Warning na každý odmítnutý request vyčerpal denní strop ingestace
		// Log Analytics (viz infra/README.md) a s ním přehled o aplikaci. Odmítnutí jsou vidět i jako
		// requesty se status kódem 429.
		_logger.LogDebug("Rate limit překročen pro klienta {ClientPartitionKey}, funkce {FunctionName}.", partitionKey, context.FunctionDefinition.Name);

		if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
		{
			httpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
		}

		// Stejný tvar odpovědi jako ostatní chyby (viz ExceptionHandlingMiddleware), nastavený přes InvocationResult.
		context.GetInvocationResult().Value = new ObjectResult(ValidationErrorModel.FromMessage(StatusCodes.Status429TooManyRequests, TooManyRequestsMessage))
		{
			StatusCode = StatusCodes.Status429TooManyRequests
		};
	}
}
