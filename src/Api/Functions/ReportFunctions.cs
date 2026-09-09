using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.Reporty;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace KandaEu.Volejbal.Api.Functions;

public class ReportFunctions(
	IReportOsobApi _reportOsobFacade,
	IReportTerminuApi _reportTerminuFacade)
{
	[Function(nameof(GetReportTerminuAsync))]
	public async Task<IActionResult> GetReportTerminuAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiRoutes.ReportyTerminy)] HttpRequest request,
		CancellationToken cancellationToken)
	{
		return new OkObjectResult(await _reportTerminuFacade.GetReportAsync(cancellationToken));
	}

	[Function(nameof(GetReportOsobAsync))]
	public async Task<IActionResult> GetReportOsobAsync(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ApiRoutes.ReportyOsoby)] HttpRequest request,
		CancellationToken cancellationToken)
	{
		return new OkObjectResult(await _reportOsobFacade.GetReportAsync(cancellationToken));
	}
}
