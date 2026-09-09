using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Terminy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace KandaEu.Volejbal.Api.Functions;

public class TerminFunctions(
	ITerminApi _terminFacade,
	IPrihlaskaApi _prihlaskaFacade)
{
	[Function(nameof(GetTerminyAsync))]
	public async Task<IActionResult> GetTerminyAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiRoutes.Terminy)] HttpRequest request,
		CancellationToken cancellationToken)
	{
		return new OkObjectResult(await _terminFacade.GetTerminyAsync(cancellationToken));
	}

	[Function(nameof(GetDetailTerminuAsync))]
	public async Task<IActionResult> GetDetailTerminuAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiRoutes.Termin)] HttpRequest request,
		int terminId,
		CancellationToken cancellationToken)
	{
		return new OkObjectResult(await _terminFacade.GetDetailTerminuAsync(terminId, cancellationToken));
	}

	[Function(nameof(PrihlasitAsync))]
	public async Task<IActionResult> PrihlasitAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ApiRoutes.TerminPrihlasit)] HttpRequest request,
		int terminId,
		int osobaId,
		CancellationToken cancellationToken)
	{
		await _prihlaskaFacade.PrihlasitAsync(terminId, osobaId, cancellationToken);
		return new NoContentResult();
	}

	[Function(nameof(OdhlasitAsync))]
	public async Task<IActionResult> OdhlasitAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ApiRoutes.TerminOdhlasit)] HttpRequest request,
		int terminId,
		int osobaId,
		CancellationToken cancellationToken)
	{
		await _prihlaskaFacade.OdhlasitAsync(terminId, osobaId, cancellationToken);
		return new NoContentResult();
	}
}
