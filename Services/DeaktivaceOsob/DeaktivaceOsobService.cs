using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.TimeServices;
using KandaEu.Volejbal.DataLayer.DataSources;
using KandaEu.Volejbal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KandaEu.Volejbal.Services.DeaktivaceOsob;

[Service]
public class DeaktivaceOsobService : IDeaktivaceOsobService
{
    private readonly IOsobaDataSource osobaDataSource;
    private readonly ITerminDataSource terminDataSource;
    private readonly IUnitOfWork unitOfWork;
    private readonly ITimeService timeService;

    public DeaktivaceOsobService(IOsobaDataSource osobaDataSource, ITerminDataSource terminDataSource, IUnitOfWork unitOfWork, ITimeService timeService)
    {
        this.osobaDataSource = osobaDataSource;
        this.terminDataSource = terminDataSource;
        this.unitOfWork = unitOfWork;
        this.timeService = timeService;
    }

    public void DeaktivujOsoby()
    {
        var osobyKDeaktivaci = GetOsobyKDeaktivaci();
        if (osobyKDeaktivaci.Any())
        {
            osobyKDeaktivaci.ForEach(osoba => osoba.Aktivni = false);
            unitOfWork.Commit();
        }
    }

    private List<Osoba> GetOsobyKDeaktivaci()
    {
        DateTime twoMonthsAgo = timeService.GetCurrentDate().AddMonths(-2);
        IQueryable<Osoba> osobySAktivitou = terminDataSource.Data.Where(item => item.Datum > twoMonthsAgo).SelectMany(item => item.Prihlasky).Where(prihlaska => prihlaska.Deleted == null).Select(prihlaska => prihlaska.Osoba).Distinct();
        IQueryable<Osoba> aktivniOsoby = osobaDataSource.Data.Where(item => item.Aktivni);

        // osoby k deaktivaci jsou ty aktivní osoby, které nemají žádnou aktivitu
        return aktivniOsoby.Except(osobySAktivitou).ToList();
    }
}
