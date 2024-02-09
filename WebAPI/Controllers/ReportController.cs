using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using Microsoft.AspNetCore.Mvc;

namespace KandaEu.Volejbal.WebAPI.Controllers;

public class ReportController(IReportOsobFacade _reportOsobFacade, IReportTerminuFacade _reportTerminuFacade)
{
	[HttpGet("/reporty/terminy")]
	public async Task<ReportTerminu> GetReportTerminuAsync(CancellationToken cancellationToken) => await _reportTerminuFacade.GetReportAsync(cancellationToken);

	[HttpGet("/reporty/osoby")]
	public async Task<ReportOsob> GetReportOsobAsync(CancellationToken cancellationToken) => await _reportOsobFacade.GetReportAsync(cancellationToken);
}
