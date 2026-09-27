using System.Net;
using KandaEu.Volejbal.DataLayer.Cosmos;
using Microsoft.Azure.Cosmos;

namespace KandaEu.Volejbal.DataLayer.Repositories;

/// <summary>
/// Termíny v kontejneru "terminy". Partition key je /id, tedy datum termínu (viz <see cref="Termin.Id" />):
/// každý termín je vlastní logickou partition, point read i zápis mají partition key vždy v ruce.
/// </summary>
/// <remarks>
/// Dotazy přes více termínů běží nutně napříč partitions. Při desítkách dokumentů na sezónu, které
/// všechny leží v jediné fyzické partition, je to zdarma - dotaz obslouží jedna partition tak jako tak.
/// </remarks>
public class TerminCosmosRepository(VolejbalCosmosContainers _containers) : ITerminRepository
{
	public async Task<Termin> GetTerminAsync(string terminId, CancellationToken cancellationToken = default)
	{
		// Id chodí z URL. Není-li to datum, termín neexistuje; zároveň tím odpadnou znaky, které Cosmos
		// v id nedovolí (400 by skončila jako 500).
		if (Termin.TryGetDatum(terminId) == null)
		{
			throw new ObjectNotFoundException("Termín nebyl nalezen.");
		}

		try
		{
			ItemResponse<Termin> response = await _containers.Terminy.ReadItemAsync<Termin>(terminId, new PartitionKey(terminId), cancellationToken: cancellationToken);
			Termin termin = response.Resource;
			termin.ETag = response.ETag;
			return termin;
		}
		catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
		{
			throw new ObjectNotFoundException("Termín nebyl nalezen.", exception);
		}
	}

	public Task<List<Termin>> GetBudouciTerminyAsync(DateTime today, CancellationToken cancellationToken = default)
	{
		return QueryAsync(
			new QueryDefinition("SELECT * FROM c WHERE c.datum >= @od AND NOT IS_DEFINED(c.deleted) ORDER BY c.datum")
				.WithParameter("@od", CosmosDateTimeConverter.ToCosmosString(today.Date)),
			cancellationToken);
	}

	public Task<List<Termin>> GetBudouciTerminyIncludingDeletedAsync(DateTime today, CancellationToken cancellationToken = default)
	{
		return QueryAsync(
			new QueryDefinition("SELECT * FROM c WHERE c.datum >= @od ORDER BY c.datum")
				.WithParameter("@od", CosmosDateTimeConverter.ToCosmosString(today.Date)),
			cancellationToken);
	}

	public async Task<DateTime?> GetPosledniDatumTerminuAsync(CancellationToken cancellationToken = default)
	{
		List<Termin> terminy = await QueryAsync(
			new QueryDefinition("SELECT TOP 1 * FROM c ORDER BY c.datum DESC"),
			cancellationToken);

		return terminy.FirstOrDefault()?.Datum;
	}

	public Task<List<Termin>> GetTerminyVObdobiAsync(DateTime odInclusive, DateTime doExclusive, CancellationToken cancellationToken = default)
	{
		return QueryAsync(
			new QueryDefinition("SELECT * FROM c WHERE c.datum >= @od AND c.datum < @do AND NOT IS_DEFINED(c.deleted) ORDER BY c.datum")
				.WithParameter("@od", CosmosDateTimeConverter.ToCosmosString(odInclusive))
				.WithParameter("@do", CosmosDateTimeConverter.ToCosmosString(doExclusive)),
			cancellationToken);
	}

	public async Task<bool> TryCreateTerminAsync(Termin termin, CancellationToken cancellationToken = default)
	{
		termin.Datum = termin.Datum.Date;
		termin.Id = Termin.GetId(termin.Datum);

		try
		{
			ItemResponse<Termin> response = await _containers.Terminy.CreateItemAsync(termin, new PartitionKey(termin.Id), cancellationToken: cancellationToken);
			termin.ETag = response.ETag;
			return true;
		}
		catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
		{
			// Termín s tímto datem už někdo založil (souběžný požadavek) - rozhodne volající.
			return false;
		}
	}

	public async Task<bool> TryReplaceTerminAsync(Termin termin, CancellationToken cancellationToken = default)
	{
		if (String.IsNullOrEmpty(termin.ETag))
		{
			// Bez ETagu by šlo o nepodmíněný přepis, který by tiše zahodil souběžné přihlášky.
			throw new InvalidOperationException("Termín nemá ETag - k úpravě je nutné ho načíst přes GetTerminAsync.");
		}

		try
		{
			ItemResponse<Termin> response = await _containers.Terminy.ReplaceItemAsync(
				termin,
				termin.Id,
				new PartitionKey(termin.Id),
				new ItemRequestOptions { IfMatchEtag = termin.ETag },
				cancellationToken);
			termin.ETag = response.ETag;
			return true;
		}
		catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.PreconditionFailed)
		{
			return false;
		}
	}

	private Task<List<Termin>> QueryAsync(QueryDefinition query, CancellationToken cancellationToken)
	{
		return _containers.Terminy.QueryToListAsync<Termin>(query, requestOptions: null, cancellationToken);
	}
}
