using System.Text.Json;
using System.Text.Json.Nodes;
using KandaEu.Volejbal.DataLayer.Cosmos;
using KandaEu.Volejbal.Model;

namespace KandaEu.Volejbal.Tests.DataLayer;

/// <summary>
/// Tvar dokumentů v Cosmos DB. Hlídá věci, na kterých stojí dotazy repozitářů a které kompilátor nevidí:
/// "id" malými písmeny, null se nezapisuje (NOT IS_DEFINED(c.deleted)),
/// pevný tvar dat (porovnávají se jako řetězce) a ETag mimo dokument.
/// </summary>
[TestClass]
public class CosmosSerializationTests
{
	private static readonly JsonSerializerOptions s_options = CosmosClientFactory.CreateSerializerOptions();

	[TestMethod]
	public void Termin_Serializace_MaIdAPrihlaskyBezETagu()
	{
		Termin termin = new Termin
		{
			Id = "2026-01-13",
			Datum = new DateTime(2026, 1, 13),
			ETag = "\"etag\"",
			Prihlasky =
			{
				new Prihlaska { OsobaId = "osoba-1", DatumPrihlaseni = new DateTime(2026, 1, 10, 18, 30, 0) },
				new Prihlaska { OsobaId = "osoba-2", DatumPrihlaseni = new DateTime(2026, 1, 11, 8, 0, 0), Deleted = new DateTime(2026, 1, 12, 9, 0, 0) }
			}
		};

		JsonObject json = JsonSerializer.SerializeToNode(termin, s_options).AsObject();

		Assert.AreEqual("2026-01-13", (string)json["id"]);
		Assert.AreEqual("2026-01-13T00:00:00.0000000", (string)json["datum"]);
		Assert.IsFalse(json.ContainsKey("deleted"), "Null se do dokumentu nezapisuje - soft-delete dotazy se ptají NOT IS_DEFINED(c.deleted).");
		Assert.IsFalse(json.ContainsKey("eTag"), "ETag je systémová vlastnost, do dokumentu nepatří.");
		Assert.IsFalse(json.ContainsKey("Id"), "Cosmos vyžaduje id malými písmeny.");

		JsonArray prihlasky = json["prihlasky"].AsArray();
		Assert.HasCount(2, prihlasky);
		Assert.IsFalse(prihlasky[0].AsObject().ContainsKey("deleted"));
		Assert.AreEqual("2026-01-12T09:00:00.0000000", (string)prihlasky[1]["deleted"]);
	}

	[TestMethod]
	public void Osoba_Serializace_BezVypoctenehoJmenaANullu()
	{
		Osoba osoba = new Osoba { Id = "id", Prijmeni = "Čapek", Jmeno = "Karel", Email = "karel@example.com" };

		JsonObject json = JsonSerializer.SerializeToNode(osoba, s_options).AsObject();

		Assert.IsTrue((bool)json["aktivni"]);
		Assert.IsFalse(json.ContainsKey("deleted"));
		Assert.IsFalse(json.ContainsKey("prijmeniJmeno"), "Vypočtené jméno se neukládá.");
	}

	[TestMethod]
	public void Vzkaz_Serializace_MaPevnyTvarData()
	{
		Vzkaz vzkaz = new Vzkaz { Id = "id", AutorId = "autor", Zprava = "Ahoj", DatumVlozeni = new DateTime(2026, 1, 13, 20, 15, 30) };

		JsonObject json = JsonSerializer.SerializeToNode(vzkaz, s_options).AsObject();

		Assert.AreEqual("2026-01-13T20:15:30.0000000", (string)json["datumVlozeni"]);
	}

	[TestMethod]
	public void DateTime_Serializace_NezavisiNaDruhuCasu()
	{
		// Dřív datetime2 bez zóny; Kind (Local/Utc/Unspecified) nesmí měnit zápis ani hodnotu,
		// jinak se rozjedou řetězcová porovnání v dotazech.
		DateTime cas = new DateTime(2026, 1, 13, 18, 30, 0);

		foreach (DateTimeKind kind in new[] { DateTimeKind.Unspecified, DateTimeKind.Local, DateTimeKind.Utc })
		{
			string json = JsonSerializer.Serialize(DateTime.SpecifyKind(cas, kind), s_options);
			Assert.AreEqual("\"2026-01-13T18:30:00.0000000\"", json, $"Kind = {kind}");
		}

		DateTime nacteny = JsonSerializer.Deserialize<DateTime>("\"2026-01-13T18:30:00.0000000\"", s_options);
		Assert.AreEqual(cas, nacteny);
		Assert.AreEqual(DateTimeKind.Unspecified, nacteny.Kind);
	}

	[TestMethod]
	public void Termin_Deserializace_NacteDatumIPrihlasky()
	{
		const string json = """{"id":"2026-01-13","datum":"2026-01-13T00:00:00.0000000","prihlasky":[{"osobaId":"o1","datumPrihlaseni":"2026-01-10T18:30:00.0000000"}],"_etag":"\"x\"","_ts":1}""";

		Termin termin = JsonSerializer.Deserialize<Termin>(json, s_options);

		Assert.AreEqual(new DateTime(2026, 1, 13), termin.Datum);
		Assert.IsNull(termin.Deleted);
		Assert.HasCount(1, termin.Prihlasky);
		Assert.IsNull(termin.Prihlasky[0].Deleted);
		Assert.IsNull(termin.ETag, "ETag naplňuje repozitář z odpovědi, ne serializace.");
	}
}
