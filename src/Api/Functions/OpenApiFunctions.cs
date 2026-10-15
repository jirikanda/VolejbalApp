using KandaEu.Volejbal.Api.OpenApi;
using KandaEu.Volejbal.Contracts.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace KandaEu.Volejbal.Api.Functions;

/// <summary>
/// OpenAPI dokument HTTP API. Vzniká při kompilaci (zdrojový generátor OpenApiGenerator nad Refit
/// rozhraními v Contracts), za běhu se jen vrací hotový text - žádná reflexe, nic navíc ve studeném startu.
/// </summary>
/// <remarks>
/// Web.Client dokument nepotřebuje (jeho klienty generuje Refit z téhož rozhraní); slouží cizím klientům
/// a nástrojům (Scalar, Swagger UI, Postman), které si ho načtou z api/openapi.json.
/// </remarks>
public class OpenApiFunctions
{
	[Function(nameof(GetOpenApiDocument))]
	public IActionResult GetOpenApiDocument(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiRoutes.OpenApi)] HttpRequest request)
	{
		return new ContentResult
		{
			Content = GeneratedOpenApiDocument.Json,
			ContentType = "application/json; charset=utf-8",
			StatusCode = StatusCodes.Status200OK
		};
	}
}
