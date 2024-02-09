using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using Microsoft.AspNetCore.Mvc;

namespace KandaEu.Volejbal.WebAPI.Controllers;

public class ReportController
{
	private readonly IReportOsobFacade reportOsobFacade;
	private readonly IReportTerminuFacade reportTerminuFacade;

	public ReportController(IReportOsobFacade reportOsobFacade, IReportTerminuFacade reportTerminuFacade)
	{
		this.reportOsobFacade = reportOsobFacade;
		this.reportTerminuFacade = reportTerminuFacade;
	}

	[HttpGet("/reporty/terminy")]
	public async Task<ReportTerminu> GetReportTerminuAsync(CancellationToken cancellationToken) => await reportTerminuFacade.GetReportAsync(cancellationToken);

	[HttpGet("/reporty/osoby")]
	public async Task<ReportOsob> GetReportOsobAsync(CancellationToken cancellationToken) => await reportOsobFacade.GetReportAsync(cancellationToken);
}
