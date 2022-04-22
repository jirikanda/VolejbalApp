using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using KandaEu.Volejbal.DataLayer.DataSources;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Havit.Extensions.DependencyInjection.Abstractions;
using KandaEu.Volejbal.Contracts.Reporty;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace KandaEu.Volejbal.Facades.Reporty;

[Service]
public class ReportTerminuFacade : IReportTerminuFacade
{
    private readonly ITerminDataSource terminDataSource;
    private readonly ITimeService timeService;

    public ReportTerminuFacade(ITerminDataSource terminDataSource, ITimeService timeService)
    {
        this.terminDataSource = terminDataSource;
        this.timeService = timeService;
    }

    public async Task<ReportTerminu> GetReport()
    {
        DateTime today = timeService.GetCurrentDate();
        DateTime datumOdInclusive = ReportHelpers.GetZacatekSkolnihoRoku(timeService);

        return new ReportTerminu
        {
            ObsazenostTerminu = await terminDataSource.Data.Where(termin => (termin.Datum >= datumOdInclusive) && (termin.Datum < today))
            .OrderBy(item => item.Datum)
            .Select(termin => new ReportTerminuItem
            {
                Datum = termin.Datum,
                PocetHracu = termin.Prihlasky.Where(prihlaska => prihlaska.Deleted == null).Count()
            }).ToListAsync()
        };
    }
}
