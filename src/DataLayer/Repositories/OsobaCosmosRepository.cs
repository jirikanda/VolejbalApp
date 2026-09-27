using System.Net;
using KandaEu.Volejbal.DataLayer.Cosmos;
using Microsoft.Azure.Cosmos;

namespace KandaEu.Volejbal.DataLayer.Repositories;

/// <summary>
/// Osoby v kontejneru "osoby". Partition key je /id, takže model nenese nic navíc a point read i zápis
/// mají klíč vždy v ruce. Seznamy běží napříč partitions - při desítkách dokumentů v jediné fyzické
/// partition je to zdarma.
/// </summary>
public class OsobaCosmosRepository(VolejbalCosmosContainers _containers) : IOsobaRepository
{
	public async Task<Osoba> GetOsobaAsync(string osobaId, CancellationToken cancellationToken = default)
	{
		// Id chodí z URL. Cosmos by pro id s nepovolenými znaky (lomítko, otazník) vrátil 400,
		// což by skončilo jako 500; naše id jsou vždy GUID, takže cokoli jiného rovnou "neexistuje".
		if (!Guid.TryParse(osobaId, out _))
		{
			throw new ObjectNotFoundException("Osoba nebyla nalezena.");
		}

		try
		{
			ItemResponse<Osoba> response = await _containers.Osoby.ReadItemAsync<Osoba>(osobaId, new PartitionKey(osobaId), cancellationToken: cancellationToken);
			return response.Resource;
		}
		catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
		{
			throw new ObjectNotFoundException("Osoba nebyla nalezena.", exception);
		}
	}

	public async Task<List<Osoba>> GetOsobyAsync(IReadOnlyCollection<string> osobaIds, CancellationToken cancellationToken = default)
	{
		// Partition key je id, takže jde o dávku point readů v jednom požadavku - levnější než dotaz
		// s ARRAY_CONTAINS. Neexistující položky Cosmos vynechá, proto se úplnost kontroluje až nad výsledkem.
		List<(string, PartitionKey)> polozky = osobaIds
			.Distinct()
			.Where(osobaId => Guid.TryParse(osobaId, out _))
			.Select(osobaId => (osobaId, new PartitionKey(osobaId)))
			.ToList();

		if (polozky.Count == 0)
		{
			// Nic platného k načtení; když přitom vstup nebyl prázdný, chybějící id nahlásí kontrola níže.
			return ThrowIfChybejici(osobaIds, new List<Osoba>());
		}

		FeedResponse<Osoba> response = await _containers.Osoby.ReadManyItemsAsync<Osoba>(polozky, cancellationToken: cancellationToken);
		return ThrowIfChybejici(osobaIds, response.ToList());
	}

	private static List<Osoba> ThrowIfChybejici(IReadOnlyCollection<string> osobaIds, List<Osoba> osoby)
	{
		HashSet<string> nalezene = osoby.Select(osoba => osoba.Id).ToHashSet();
		List<string> chybejici = osobaIds.Where(osobaId => !nalezene.Contains(osobaId)).Distinct().ToList();
		if (chybejici.Count > 0)
		{
			throw new ObjectNotFoundException($"Osoby nebyly nalezeny: {String.Join(", ", chybejici)}.");
		}

		return osoby;
	}

	public Task<List<Osoba>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return QueryAsync("SELECT * FROM c WHERE NOT IS_DEFINED(c.deleted)", cancellationToken);
	}

	public Task<List<Osoba>> GetAllAktivniAsync(CancellationToken cancellationToken = default)
	{
		return QueryAsync("SELECT * FROM c WHERE NOT IS_DEFINED(c.deleted) AND c.aktivni = true", cancellationToken);
	}

	public async Task InsertAsync(Osoba osoba, CancellationToken cancellationToken = default)
	{
		osoba.Id ??= Guid.NewGuid().ToString();
		await _containers.Osoby.CreateItemAsync(osoba, new PartitionKey(osoba.Id), cancellationToken: cancellationToken);
	}

	public async Task UpdateAsync(Osoba osoba, CancellationToken cancellationToken = default)
	{
		// Bez ETagu: osoby mění jen správce v seznamu hráčů, souběh dvou správců je teoretický
		// a poslední zápis vyhraje - stejně jako dřív v SQL.
		await _containers.Osoby.ReplaceItemAsync(osoba, osoba.Id, new PartitionKey(osoba.Id), cancellationToken: cancellationToken);
	}

	private async Task<List<Osoba>> QueryAsync(string sql, CancellationToken cancellationToken)
	{
		List<Osoba> osoby = await _containers.Osoby.QueryToListAsync<Osoba>(new QueryDefinition(sql), requestOptions: null, cancellationToken);

		// České řazení v paměti - ORDER BY v Cosmosu řadí ordinálně (Čapek až za Zemanem).
		return osoby.OrderByPrijmeniJmeno().ToList();
	}
}
