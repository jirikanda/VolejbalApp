using KandaEu.Volejbal.DataLayer.Cosmos;
using KandaEu.Volejbal.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.MigrationTool;

/// <summary>
/// Jednorázový převod dat z původní SQL databáze (tabulky Osoba, Termin, Prihlaska, Vzkaz) do Cosmos DB.
/// </summary>
/// <remarks>
/// Čte přes ADO.NET - projekt Entity s EF modelem už neexistuje. Řádky přihlášek se skládají do pole
/// v dokumentu termínu.
///
/// Import je opakovatelný: nová id jsou odvozená z původních číselných (<see cref="GetId" />), ne
/// náhodná, a zapisuje se upsertem - druhý průchod tedy dokumenty přepíše, ne zduplikuje.
/// </remarks>
public class SqlImport(VolejbalCosmosContainers _containers, ILogger<SqlImport> _logger)
{
	public async Task ImportAsync(string sqlConnectionString, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Čtu data ze SQL Serveru...");

		Dictionary<int, Osoba> osoby;
		Dictionary<int, Termin> terminy;
		List<Vzkaz> vzkazy;

		await using (SqlConnection connection = new SqlConnection(sqlConnectionString))
		{
			await connection.OpenAsync(cancellationToken);

			osoby = await ReadOsobyAsync(connection, cancellationToken);
			terminy = await ReadTerminyAsync(connection, cancellationToken);
			await ReadPrihlaskyAsync(connection, terminy, osoby, cancellationToken);
			vzkazy = await ReadVzkazyAsync(connection, osoby, cancellationToken);
		}

		_logger.LogInformation("Načteno: {osoby} osob, {terminy} termínů, {vzkazy} vzkazů. Zapisuji do Cosmos DB...", osoby.Count, terminy.Count, vzkazy.Count);

		foreach (Osoba osoba in osoby.Values)
		{
			await _containers.Osoby.UpsertItemAsync(osoba, new PartitionKey(osoba.Id), cancellationToken: cancellationToken);
		}

		foreach (Termin termin in terminy.Values)
		{
			await _containers.Terminy.UpsertItemAsync(termin, new PartitionKey(termin.Id), cancellationToken: cancellationToken);
		}

		foreach (Vzkaz vzkaz in vzkazy)
		{
			await _containers.Vzkazy.UpsertItemAsync(vzkaz, new PartitionKey(vzkaz.Id), cancellationToken: cancellationToken);
		}

		_logger.LogInformation("Převod dat dokončen.");
	}

	/// <summary>
	/// Id dokumentu odvozené z původního číselného id - deterministické, aby šel import opakovat.
	/// Tvarem je to GUID, protože repozitáře id jako GUID validují.
	/// </summary>
	internal static string GetId(int sqlId)
	{
		return new Guid(sqlId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0).ToString();
	}

	private static async Task<Dictionary<int, Osoba>> ReadOsobyAsync(SqlConnection connection, CancellationToken cancellationToken)
	{
		Dictionary<int, Osoba> result = new Dictionary<int, Osoba>();

		await using SqlCommand command = new SqlCommand("SELECT Id, Prijmeni, Jmeno, Email, Deleted, Aktivni FROM Osoba", connection);
		await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			int sqlId = reader.GetInt32(0);
			result.Add(sqlId, new Osoba
			{
				Id = GetId(sqlId),
				Prijmeni = reader.GetString(1),
				Jmeno = reader.GetString(2),
				Email = reader.GetString(3),
				Deleted = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
				Aktivni = reader.GetBoolean(5)
			});
		}

		return result;
	}

	private async Task<Dictionary<int, Termin>> ReadTerminyAsync(SqlConnection connection, CancellationToken cancellationToken)
	{
		// Klíč je původní SQL id (kvůli přihláškám), dokument termínu je ale identifikovaný datem.
		// Unikátní index UIDX_Termin_Datum_Deleted dovoloval víc smazaných termínů téhož data (lišily se
		// časem smazání). V Cosmosu je na datum jeden dokument, takže pro každé datum vyhrává nesmazaný
		// termín, jinak naposledy smazaný - a do slovníku jde JEN vítěz. Přihlášky poražených výskytů se
		// tím nepřenesou; slít je do vítěze by hráče přihlásilo na termín, na který se nepřihlásili.
		List<(int SqlId, DateTime Datum, DateTime? Deleted)> radky = new List<(int, DateTime, DateTime?)>();

		await using (SqlCommand command = new SqlCommand("SELECT Id, Datum, Deleted FROM Termin", connection))
		await using (SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken))
		{
			while (await reader.ReadAsync(cancellationToken))
			{
				radky.Add((reader.GetInt32(0), reader.GetDateTime(1).Date, reader.IsDBNull(2) ? null : reader.GetDateTime(2)));
			}
		}

		Dictionary<int, Termin> result = new Dictionary<int, Termin>();
		foreach (IGrouping<DateTime, (int SqlId, DateTime Datum, DateTime? Deleted)> skupina in radky.GroupBy(radek => radek.Datum))
		{
			(int SqlId, DateTime Datum, DateTime? Deleted) vitez = skupina
				.OrderBy(radek => radek.Deleted.HasValue) // nesmazaný první
				.ThenByDescending(radek => radek.Deleted)
				.First();

			foreach ((int SqlId, DateTime Datum, DateTime? Deleted) porazeny in skupina.Where(radek => radek.SqlId != vitez.SqlId))
			{
				_logger.LogWarning("Termín {datum} je v SQL vícekrát; výskyt id {sqlId} (smazaný {deleted}) se přeskakuje včetně jeho přihlášek, přenáší se id {vitezId}.", Termin.GetId(skupina.Key), porazeny.SqlId, porazeny.Deleted, vitez.SqlId);
			}

			result.Add(vitez.SqlId, new Termin
			{
				Id = Termin.GetId(skupina.Key),
				Datum = skupina.Key,
				Deleted = vitez.Deleted
			});
		}

		return result;
	}

	private async Task ReadPrihlaskyAsync(SqlConnection connection, Dictionary<int, Termin> terminy, Dictionary<int, Osoba> osoby, CancellationToken cancellationToken)
	{
		// Na osobu a termín mohlo být v SQL víc řádků (nejvýš jedna přihláška plus historie odhlášek
		// s různým časem smazání). V dokumentu je na osobu jedna položka: přihláška, pokud existuje,
		// jinak poslední odhláška (tombstone, který UI ukazuje jako "odhlášený").
		await using SqlCommand command = new SqlCommand("SELECT TerminId, OsobaId, DatumPrihlaseni, Deleted FROM Prihlaska ORDER BY TerminId, OsobaId, Deleted DESC", connection);
		await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			int terminId = reader.GetInt32(0);
			int osobaId = reader.GetInt32(1);
			DateTime datumPrihlaseni = reader.GetDateTime(2);
			DateTime? deleted = reader.IsDBNull(3) ? null : reader.GetDateTime(3);

			if (!terminy.TryGetValue(terminId, out Termin termin) || !osoby.TryGetValue(osobaId, out Osoba osoba))
			{
				// Termín buď neexistuje, nebo je to poražený duplicitní výskyt (viz ReadTerminyAsync).
				_logger.LogWarning("Přihláška (termín {terminId}, osoba {osobaId}) odkazuje na nepřenášený záznam, přeskakuji.", terminId, osobaId);
				continue;
			}

			Prihlaska existujici = termin.Prihlasky.FirstOrDefault(prihlaska => prihlaska.OsobaId == osoba.Id);
			if (existujici == null)
			{
				termin.Prihlasky.Add(new Prihlaska
				{
					OsobaId = osoba.Id,
					DatumPrihlaseni = datumPrihlaseni,
					Deleted = deleted
				});
			}
			else if ((existujici.Deleted != null) && ((deleted == null) || (deleted > existujici.Deleted)))
			{
				// Aktivní přihláška má přednost před odhláškou, novější odhláška před starší.
				existujici.DatumPrihlaseni = datumPrihlaseni;
				existujici.Deleted = deleted;
			}
		}
	}

	private async Task<List<Vzkaz>> ReadVzkazyAsync(SqlConnection connection, Dictionary<int, Osoba> osoby, CancellationToken cancellationToken)
	{
		List<Vzkaz> result = new List<Vzkaz>();

		await using SqlCommand command = new SqlCommand("SELECT Id, AutorId, DatumVlozeni, Zprava, Deleted FROM Vzkaz", connection);
		await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
		while (await reader.ReadAsync(cancellationToken))
		{
			int sqlId = reader.GetInt32(0);
			int autorId = reader.GetInt32(1);

			if (!osoby.TryGetValue(autorId, out Osoba autor))
			{
				_logger.LogWarning("Vzkaz {sqlId} odkazuje na neexistujícího autora {autorId}, přeskakuji.", sqlId, autorId);
				continue;
			}

			result.Add(new Vzkaz
			{
				Id = GetId(sqlId),
				AutorId = autor.Id,
				DatumVlozeni = reader.GetDateTime(2),
				Zprava = reader.GetString(3),
				Deleted = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
			});
		}

		return result;
	}
}
