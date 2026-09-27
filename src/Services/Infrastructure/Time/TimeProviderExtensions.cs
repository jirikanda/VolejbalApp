namespace KandaEu.Volejbal.Services.Infrastructure.Time;

/// <summary>
/// Místní čas jako <see cref="DateTime" /> bez zóny - tvar, ve kterém aplikace s časem pracuje
/// a ukládá ho (dokumenty nesou pražský wall-clock čas, viz CosmosDateTimeConverter).
/// </summary>
public static class TimeProviderExtensions
{
	/// <summary>
	/// Aktuální místní čas (podle <see cref="TimeProvider.LocalTimeZone" />) bez informace o zóně.
	/// </summary>
	public static DateTime GetLocalDateTime(this TimeProvider timeProvider)
	{
		return timeProvider.GetLocalNow().DateTime;
	}

	/// <summary>
	/// Dnešní místní datum (půlnoc, bez času).
	/// </summary>
	public static DateTime GetLocalToday(this TimeProvider timeProvider)
	{
		return timeProvider.GetLocalNow().Date;
	}
}
