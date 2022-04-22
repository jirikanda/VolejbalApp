using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using Microsoft.EntityFrameworkCore;

namespace KandaEu.Volejbal.Facades.Reporty;

[Service]
public class ReportOsobFacade : IReportOsobFacade
{
    private readonly IOsobaDataSource osobaDataSource;
    private readonly ITimeService timeService;

    public ReportOsobFacade(IOsobaDataSource osobaDataSource, ITimeService timeService)
    {
        this.osobaDataSource = osobaDataSource;
        this.timeService = timeService;
    }

    public async Task<ReportOsob> GetReport()
    {
        DateTime today = timeService.GetCurrentDate();
        DateTime datumOdInclusive = ReportHelpers.GetZacatekSkolnihoRoku(timeService);

        return new ReportOsob
        {
            UcastHracu = (await osobaDataSource.Data.Select(osoba =>
            new ReportOsobItem
            {
                PrijmeniJmeno = osoba.PrijmeniJmeno,
                PocetTerminu = osoba.Prihlasky.Where(prihlaska => (prihlaska.Termin.Datum >= datumOdInclusive) && (prihlaska.Termin.Datum < today) && (prihlaska.Termin.Deleted == null) && (prihlaska.Deleted == null)).Count()
            })
            .ToListAsync())
            .Where(item => item.PocetTerminu > 0) // in memory
            .OrderBy(item => item.PrijmeniJmeno)
            .ToList()
        };
    }
}
