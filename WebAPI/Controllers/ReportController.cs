using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using KandaEu.Volejbal.Facades.Reporty;
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
    public async Task<ReportTerminu> GetReportTerminu() => await reportTerminuFacade.GetReport();

    [HttpGet("/reporty/osoby")]
    public async Task<ReportOsob> GetReportOsob() => await reportOsobFacade.GetReport();
}
