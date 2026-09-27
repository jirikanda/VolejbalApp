namespace KandaEu.Volejbal.Services.Infrastructure.Time;

/// <summary>
/// Poskytuje aktuální čas v české časové zóně. Registruje se jako <see cref="TimeProvider" />, veškerý
/// serverový kód si "teď" a "dnes" bere z něj, ne z DateTime.Now.
/// </summary>
/// <remarks>
/// Zóna je určena v kódu, ne proměnnou TZ - Azure Functions Flex Consumption nastavení TZ (ani
/// WEBSITE_TIME_ZONE) nepodporuje, takže proces běží v UTC a <see cref="TimeProvider.System" /> by po
/// 22:00 SELČ vracel jiný den, než mají hráči na hodinkách.
///
/// Použito IANA ID "Europe/Prague", ne windowsí "Central Europe Standard Time": produkce běží na
/// Linuxu, kde je IANA nativní. .NET umí obě ID na obou platformách, ale jen dokud je k dispozici
/// ICU - v invariant globalization módu by windowsí ID na Linuxu selhalo.
/// </remarks>
public class PragueTimeProvider : TimeProvider
{
	private static readonly TimeZoneInfo s_timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Prague");

	public override TimeZoneInfo LocalTimeZone => s_timeZone;
}
