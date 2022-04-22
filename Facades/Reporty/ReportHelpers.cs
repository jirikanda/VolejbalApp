using Havit.Services.TimeServices;

namespace KandaEu.Volejbal.Facades.Reporty;

public static class ReportHelpers
{
    public static DateTime GetZacatekSkolnihoRoku(ITimeService timeService)
    {
        DateTime today = timeService.GetCurrentDate();
        return new DateTime(((today.Month < 9) ? today.Year - 1 /* pokud je dnes 1.-8. měsíc, začal školní rok vloni */ : today.Year /* školní rok začal letos */), 9, 1);
    }
}
