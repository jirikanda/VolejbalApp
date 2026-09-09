using KandaEu.Volejbal.Contracts.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace KandaEu.Volejbal.Api.Functions;

/// <summary>
/// Health endpoint. Záměrně nekontroluje závislosti (databázi) - odpovídá 200, jakmile stojí aplikace.
/// </summary>
/// <remarks>
/// Flex Consumption nemá health probes jako Azure Container Apps, endpoint tedy neslouží platformě,
/// ale ručnímu ověření a měření studeného startu.
/// </remarks>
public class HealthFunctions
{
	[Function(nameof(GetHealth))]
	public IActionResult GetHealth(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiRoutes.Health)] HttpRequest request)
	{
		return new OkResult();
	}
}
