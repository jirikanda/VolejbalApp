using Havit.Services.TimeServices;

namespace KandaEu.Volejbal.Services.Infrastructure.TimeService;

/// <summary>
/// Poskytuje aktuální čas v české časové zóně.
/// </summary>
/// <remarks>
/// Zóna je určena v kódu, ne proměnnou TZ - Azure Functions Flex Consumption nastavení TZ (ani
/// WEBSITE_TIME_ZONE) nepodporuje, takže proces běží v UTC. Veškerý serverový kód proto musí čas
/// brát odsud, ne z DateTime.Now.
///
/// Použito IANA ID "Europe/Prague", ne windowsí "Central Europe Standard Time": produkce běží na
/// Linuxu, kde je IANA nativní. .NET 6+ umí obě ID na obou platformách, ale jen dokud je k dispozici
/// ICU - v invariant globalization módu by windowsí ID na Linuxu selhalo.
/// </remarks>
public class ApplicationTimeService : TimeZoneTimeServiceBase, ITimeService
{
	/// <summary>
	/// Aktuální čas v časové zóně aplikace
	/// </summary>
	public static DateTime LocalNow => new ApplicationTimeService().GetCurrentTime();

	/// <summary>
	/// Vrací časovou zónu, pro kterou je poskytován aktuální čas. Vždy česká časová zóna.
	/// </summary>
	protected override TimeZoneInfo CurrentTimeZone => TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague");
}
