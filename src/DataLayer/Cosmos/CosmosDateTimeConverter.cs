using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KandaEu.Volejbal.DataLayer.Cosmos;

/// <summary>
/// Serializace DateTime do dokumentů: vždy stejný tvar "2026-01-13T18:30:00.0000000", bez časové zóny.
/// </summary>
/// <remarks>
/// Výchozí System.Text.Json připojuje podle DateTime.Kind buď "Z", nebo posun ("+01:00"), nebo nic.
/// Cosmos ale porovnává data jako řetězce (JSON žádný datový typ pro datum nemá), takže dotazy
/// <c>c.datum &gt;= @od</c> fungují jen tehdy, když mají všechny hodnoty tentýž pevný tvar. Hodnoty jsou
/// místní (pražský) čas bez zóny, přesně jako dřív ve sloupcích datetime2 - <see cref="Format" />
/// je proto i tvar parametrů dotazů, viz <see cref="ToCosmosString" />.
/// </remarks>
public sealed class CosmosDateTimeConverter : JsonConverter<DateTime>
{
	private const string Format = "yyyy-MM-ddTHH:mm:ss.fffffff";

	/// <summary>
	/// Hodnota v tvaru, v jakém leží v dokumentech - pro parametry dotazů porovnávajících data.
	/// </summary>
	public static string ToCosmosString(DateTime value)
	{
		return value.ToString(Format, CultureInfo.InvariantCulture);
	}

	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// RoundtripKind: hodnota bez zóny zůstane Unspecified a nepřepočítá se podle zóny procesu (UTC v Azure).
		return DateTime.Parse(reader.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(ToCosmosString(value));
	}
}
