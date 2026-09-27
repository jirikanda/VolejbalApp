using System.Data;
using KandaEu.Volejbal.DataLayer;
using KandaEu.Volejbal.DataLayer.Cosmos;
using KandaEu.Volejbal.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.MigrationTool;

/// <summary>
/// Cesta zpět: obsah Cosmos DB do prázdné SQL databáze ve tvaru, jaký měla aplikace před přechodem
/// na Cosmos (včetně unikátních indexů a řádků __EFMigrationsHistory, aby se starší build nepokoušel
/// databázi migrovat znovu).
/// </summary>
/// <remarks>
/// Původní číselná id v Cosmosu nejsou (osoby a vzkazy mají GUID, termíny datum), takže identity sloupce
/// přidělí nová a odkazy se přemapují za letu. Přihlášky se rozbalují z pole v dokumentu termínu zpět
/// na řádky. Celý běh včetně DDL je jedna transakce - po chybě zůstane cíl prázdný.
/// </remarks>
public class CosmosExport(VolejbalCosmosContainers _containers, ILogger<CosmosExport> _logger)
{
	private static readonly string[] s_tabulkyAplikace = ["Osoba", "Termin", "Prihlaska", "Vzkaz", "__DataSeed", "__EFMigrationsHistory"];

	public async Task ExportAsync(string sqlConnectionString, CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Čtu data z Cosmos DB...");
		List<Osoba> osoby = await _containers.Osoby.QueryToListAsync<Osoba>(new QueryDefinition("SELECT * FROM c"), requestOptions: null, cancellationToken);
		List<Termin> terminy = await _containers.Terminy.QueryToListAsync<Termin>(new QueryDefinition("SELECT * FROM c"), requestOptions: null, cancellationToken);
		List<Vzkaz> vzkazy = await _containers.Vzkazy.QueryToListAsync<Vzkaz>(new QueryDefinition("SELECT * FROM c"), requestOptions: null, cancellationToken);
		_logger.LogInformation("Načteno: {osoby} osob, {terminy} termínů, {vzkazy} vzkazů.", osoby.Count, terminy.Count, vzkazy.Count);

		await using SqlConnection connection = new SqlConnection(sqlConnectionString);
		await connection.OpenAsync(cancellationToken);

		await ThrowIfTablesExistAsync(connection, cancellationToken);

		await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

		_logger.LogInformation("Zakládám schéma...");
		await ExecuteAsync(connection, transaction, SchemaSql, cancellationToken);

		_logger.LogInformation("Zapisuji data...");
		Dictionary<string, int> osobaIds = await InsertOsobyAsync(connection, transaction, osoby, cancellationToken);
		Dictionary<string, int> terminIds = await InsertTerminyAsync(connection, transaction, terminy, cancellationToken);
		int pocetPrihlasek = await InsertPrihlaskyAsync(connection, transaction, terminy, terminIds, osobaIds, cancellationToken);
		int pocetVzkazu = await InsertVzkazyAsync(connection, transaction, vzkazy, osobaIds, cancellationToken);

		await ExecuteAsync(connection, transaction, MigrationsHistorySql, cancellationToken);

		await transaction.CommitAsync(cancellationToken);
		_logger.LogInformation("Export dokončen: {osoby} osob, {terminy} termínů, {prihlasky} přihlášek, {vzkazy} vzkazů.", osobaIds.Count, terminIds.Count, pocetPrihlasek, pocetVzkazu);
	}

	private static async Task ThrowIfTablesExistAsync(SqlConnection connection, CancellationToken cancellationToken)
	{
		string tableList = String.Join(", ", s_tabulkyAplikace.Select(name => $"'{name}'"));
		await using SqlCommand command = new SqlCommand($"SELECT COUNT(*) FROM sys.tables WHERE name IN ({tableList})", connection);
		int pocet = (int)await command.ExecuteScalarAsync(cancellationToken);
		if (pocet > 0)
		{
			throw new InvalidOperationException("Cílová databáze už obsahuje tabulky aplikace. Export vyžaduje prázdnou databázi.");
		}
	}

	private static async Task<Dictionary<string, int>> InsertOsobyAsync(SqlConnection connection, SqlTransaction transaction, List<Osoba> osoby, CancellationToken cancellationToken)
	{
		Dictionary<string, int> result = new Dictionary<string, int>();

		await using SqlCommand command = new SqlCommand(
			"INSERT INTO Osoba (Prijmeni, Jmeno, Email, Deleted, Aktivni) OUTPUT INSERTED.Id VALUES (@Prijmeni, @Jmeno, @Email, @Deleted, @Aktivni)",
			connection,
			transaction);
		command.Parameters.Add("@Prijmeni", SqlDbType.NVarChar, 50);
		command.Parameters.Add("@Jmeno", SqlDbType.NVarChar, 50);
		command.Parameters.Add("@Email", SqlDbType.NVarChar, 50);
		command.Parameters.Add("@Deleted", SqlDbType.DateTime2);
		command.Parameters.Add("@Aktivni", SqlDbType.Bit);

		foreach (Osoba osoba in osoby.OrderByPrijmeniJmeno())
		{
			command.Parameters["@Prijmeni"].Value = osoba.Prijmeni;
			command.Parameters["@Jmeno"].Value = osoba.Jmeno;
			command.Parameters["@Email"].Value = osoba.Email;
			command.Parameters["@Deleted"].Value = (object)osoba.Deleted ?? DBNull.Value;
			command.Parameters["@Aktivni"].Value = osoba.Aktivni;

			result.Add(osoba.Id, (int)await command.ExecuteScalarAsync(cancellationToken));
		}

		return result;
	}

	private static async Task<Dictionary<string, int>> InsertTerminyAsync(SqlConnection connection, SqlTransaction transaction, List<Termin> terminy, CancellationToken cancellationToken)
	{
		Dictionary<string, int> result = new Dictionary<string, int>();

		await using SqlCommand command = new SqlCommand(
			"INSERT INTO Termin (Datum, Deleted) OUTPUT INSERTED.Id VALUES (@Datum, @Deleted)",
			connection,
			transaction);
		command.Parameters.Add("@Datum", SqlDbType.DateTime2);
		command.Parameters.Add("@Deleted", SqlDbType.DateTime2);

		foreach (Termin termin in terminy.OrderBy(termin => termin.Datum))
		{
			command.Parameters["@Datum"].Value = termin.Datum;
			command.Parameters["@Deleted"].Value = (object)termin.Deleted ?? DBNull.Value;

			result.Add(termin.Id, (int)await command.ExecuteScalarAsync(cancellationToken));
		}

		return result;
	}

	private async Task<int> InsertPrihlaskyAsync(SqlConnection connection, SqlTransaction transaction, List<Termin> terminy, Dictionary<string, int> terminIds, Dictionary<string, int> osobaIds, CancellationToken cancellationToken)
	{
		int pocet = 0;

		await using SqlCommand command = new SqlCommand(
			"INSERT INTO Prihlaska (TerminId, OsobaId, DatumPrihlaseni, Deleted) VALUES (@TerminId, @OsobaId, @DatumPrihlaseni, @Deleted)",
			connection,
			transaction);
		command.Parameters.Add("@TerminId", SqlDbType.Int);
		command.Parameters.Add("@OsobaId", SqlDbType.Int);
		command.Parameters.Add("@DatumPrihlaseni", SqlDbType.DateTime2);
		command.Parameters.Add("@Deleted", SqlDbType.DateTime2);

		foreach (Termin termin in terminy)
		{
			foreach (Prihlaska prihlaska in termin.Prihlasky)
			{
				if (!osobaIds.TryGetValue(prihlaska.OsobaId, out int osobaId))
				{
					_logger.LogWarning("Přihláška na termín {termin} odkazuje na neexistující osobu {osobaId}, přeskakuji.", termin.Id, prihlaska.OsobaId);
					continue;
				}

				command.Parameters["@TerminId"].Value = terminIds[termin.Id];
				command.Parameters["@OsobaId"].Value = osobaId;
				command.Parameters["@DatumPrihlaseni"].Value = prihlaska.DatumPrihlaseni;
				command.Parameters["@Deleted"].Value = (object)prihlaska.Deleted ?? DBNull.Value;

				await command.ExecuteNonQueryAsync(cancellationToken);
				pocet++;
			}
		}

		return pocet;
	}

	private async Task<int> InsertVzkazyAsync(SqlConnection connection, SqlTransaction transaction, List<Vzkaz> vzkazy, Dictionary<string, int> osobaIds, CancellationToken cancellationToken)
	{
		int pocet = 0;

		await using SqlCommand command = new SqlCommand(
			"INSERT INTO Vzkaz (AutorId, DatumVlozeni, Zprava, Deleted) VALUES (@AutorId, @DatumVlozeni, @Zprava, @Deleted)",
			connection,
			transaction);
		command.Parameters.Add("@AutorId", SqlDbType.Int);
		command.Parameters.Add("@DatumVlozeni", SqlDbType.DateTime2);
		command.Parameters.Add("@Zprava", SqlDbType.NVarChar, -1);
		command.Parameters.Add("@Deleted", SqlDbType.DateTime2);

		foreach (Vzkaz vzkaz in vzkazy.OrderBy(vzkaz => vzkaz.DatumVlozeni))
		{
			if (!osobaIds.TryGetValue(vzkaz.AutorId, out int autorId))
			{
				_logger.LogWarning("Vzkaz {vzkaz} odkazuje na neexistujícího autora {autorId}, přeskakuji.", vzkaz.Id, vzkaz.AutorId);
				continue;
			}

			command.Parameters["@AutorId"].Value = autorId;
			command.Parameters["@DatumVlozeni"].Value = vzkaz.DatumVlozeni;
			command.Parameters["@Zprava"].Value = vzkaz.Zprava;
			command.Parameters["@Deleted"].Value = (object)vzkaz.Deleted ?? DBNull.Value;

			await command.ExecuteNonQueryAsync(cancellationToken);
			pocet++;
		}

		return pocet;
	}

	private static async Task ExecuteAsync(SqlConnection connection, SqlTransaction transaction, string sql, CancellationToken cancellationToken)
	{
		await using SqlCommand command = new SqlCommand(sql, connection, transaction);
		await command.ExecuteNonQueryAsync(cancellationToken);
	}

	/// <summary>
	/// Schéma přesně podle EF migrací Initial + Osoba_Aktivni (viz historie repozitáře, src/Entity/Migrations).
	/// </summary>
	private const string SchemaSql = """
		CREATE TABLE [__EFMigrationsHistory] (
			[MigrationId] nvarchar(150) NOT NULL,
			[ProductVersion] nvarchar(32) NOT NULL,
			CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
		);

		CREATE TABLE [__DataSeed] (
			[ProfileName] nvarchar(250) NOT NULL,
			[Version] nvarchar(max) NULL,
			CONSTRAINT [PK_DataSeed] PRIMARY KEY ([ProfileName])
		);

		CREATE TABLE [Osoba] (
			[Id] int NOT NULL IDENTITY,
			[Prijmeni] nvarchar(50) NOT NULL,
			[Jmeno] nvarchar(50) NOT NULL,
			[Email] nvarchar(50) NOT NULL,
			[Deleted] datetime2 NULL,
			[Aktivni] bit NOT NULL,
			CONSTRAINT [PK_Osoba] PRIMARY KEY ([Id])
		);

		CREATE TABLE [Termin] (
			[Id] int NOT NULL IDENTITY,
			[Datum] datetime2 NOT NULL,
			[Deleted] datetime2 NULL,
			CONSTRAINT [PK_Termin] PRIMARY KEY ([Id])
		);

		CREATE TABLE [Vzkaz] (
			[Id] int NOT NULL IDENTITY,
			[AutorId] int NOT NULL,
			[DatumVlozeni] datetime2 NOT NULL,
			[Zprava] nvarchar(max) NOT NULL,
			[Deleted] datetime2 NULL,
			CONSTRAINT [PK_Vzkaz] PRIMARY KEY ([Id]),
			CONSTRAINT [FK_Vzkaz_Osoba_AutorId] FOREIGN KEY ([AutorId]) REFERENCES [Osoba] ([Id]) ON DELETE NO ACTION
		);

		CREATE TABLE [Prihlaska] (
			[Id] int NOT NULL IDENTITY,
			[OsobaId] int NOT NULL,
			[TerminId] int NOT NULL,
			[DatumPrihlaseni] datetime2 NOT NULL,
			[Deleted] datetime2 NULL,
			CONSTRAINT [PK_Prihlaska] PRIMARY KEY ([Id]),
			CONSTRAINT [FK_Prihlaska_Osoba_OsobaId] FOREIGN KEY ([OsobaId]) REFERENCES [Osoba] ([Id]) ON DELETE NO ACTION,
			CONSTRAINT [FK_Prihlaska_Termin_TerminId] FOREIGN KEY ([TerminId]) REFERENCES [Termin] ([Id]) ON DELETE NO ACTION
		);

		CREATE INDEX [IX_Prihlaska_OsobaId] ON [Prihlaska] ([OsobaId]);
		CREATE UNIQUE INDEX [UIDX_Prihlaska_TerminId_OsobaId_Deleted] ON [Prihlaska] ([TerminId], [OsobaId], [Deleted]);
		CREATE UNIQUE INDEX [UIDX_Termin_Datum_Deleted] ON [Termin] ([Datum], [Deleted]);
		CREATE INDEX [IX_Vzkaz_AutorId] ON [Vzkaz] ([AutorId]);
		""";

	/// <summary>
	/// Záznamy o aplikovaných migracích, aby starší build databázi považoval za aktuální.
	/// </summary>
	private const string MigrationsHistorySql = """
		INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES
			('20190218093705_Initial', '3.1.1'),
			('20200206094104_Osoba_Aktivni', '3.1.1');
		""";
}
