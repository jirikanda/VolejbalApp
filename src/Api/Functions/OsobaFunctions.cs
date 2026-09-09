using KandaEu.Volejbal.Api.Infrastructure;
using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace KandaEu.Volejbal.Api.Functions;

public class OsobaFunctions(IOsobaApi _osobaFacade)
{
	[Function(nameof(VlozOsobuAsync))]
	public async Task<IActionResult> VlozOsobuAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ApiRoutes.Osoby)] HttpRequest request,
		CancellationToken cancellationToken)
	{
		(bool isValid, OsobaInputDto osobaInputDto, IActionResult errorResult) = await RequestBodyReader.ReadValidatedAsync<OsobaInputDto>(request, cancellationToken);
		if (!isValid)
		{
			return errorResult;
		}

		await _osobaFacade.VlozOsobuAsync(osobaInputDto, cancellationToken);
		return new NoContentResult();
	}

	[Function(nameof(GetOsobyAsync))]
	public async Task<IActionResult> GetOsobyAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiRoutes.Osoby)] HttpRequest request,
		CancellationToken cancellationToken)
	{
		return new OkObjectResult(await _osobaFacade.GetOsobyAsync(cancellationToken));
	}

	[Function(nameof(GetAktivniOsobyAsync))]
	public async Task<IActionResult> GetAktivniOsobyAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiRoutes.OsobyAktivni)] HttpRequest request,
		CancellationToken cancellationToken)
	{
		return new OkObjectResult(await _osobaFacade.GetAktivniOsobyAsync(cancellationToken));
	}

	[Function(nameof(SmazOsobuAsync))]
	public async Task<IActionResult> SmazOsobuAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = ApiRoutes.Osoba)] HttpRequest request,
		int osobaId,
		CancellationToken cancellationToken)
	{
		await _osobaFacade.SmazOsobuAsync(osobaId, cancellationToken);
		return new NoContentResult();
	}

	[Function(nameof(AktivujOsobuAsync))]
	public async Task<IActionResult> AktivujOsobuAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ApiRoutes.OsobaAktivovat)] HttpRequest request,
		int osobaId,
		CancellationToken cancellationToken)
	{
		await _osobaFacade.AktivujOsobuAsync(osobaId, cancellationToken);
		return new NoContentResult();
	}

	[Function(nameof(DeaktivujOsobuAsync))]
	public async Task<IActionResult> DeaktivujOsobuAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ApiRoutes.OsobaDeaktivovat)] HttpRequest request,
		int osobaId,
		CancellationToken cancellationToken)
	{
		await _osobaFacade.DeaktivujOsobuAsync(osobaId, cancellationToken);
		return new NoContentResult();
	}
}
