using KandaEu.Volejbal.Api.Infrastructure;
using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace KandaEu.Volejbal.Api.Functions;

public class NastenkaFunctions(INastenkaApi _nastenkaFacade)
{
	[Function(nameof(GetVzkazyAsync))]
	public async Task<IActionResult> GetVzkazyAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiRoutes.Nastenka)] HttpRequest request,
		CancellationToken cancellationToken)
	{
		return new OkObjectResult(await _nastenkaFacade.GetVzkazyAsync(cancellationToken));
	}

	[Function(nameof(VlozVzkazAsync))]
	public async Task<IActionResult> VlozVzkazAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ApiRoutes.Nastenka)] HttpRequest request,
		CancellationToken cancellationToken)
	{
		(bool isValid, VzkazInputDto vzkaz, IActionResult errorResult) = await RequestBodyReader.ReadValidatedAsync<VzkazInputDto>(request, cancellationToken);
		if (!isValid)
		{
			return errorResult;
		}

		await _nastenkaFacade.VlozVzkazAsync(vzkaz, cancellationToken);
		return new NoContentResult();
	}
}
