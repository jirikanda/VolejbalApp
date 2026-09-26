using KandaEu.Volejbal.DataLayer.Cosmos;
using Microsoft.Azure.Cosmos;

namespace KandaEu.Volejbal.DataLayer.Repositories;

/// <summary>
/// Vzkazy v kontejneru "vzkazy". Partition key je /id; nástěnka se čte dotazem napříč partitions,
/// což je při desítkách dokumentů v jediné fyzické partition zdarma.
/// </summary>
public class VzkazCosmosRepository(VolejbalCosmosContainers _containers) : IVzkazRepository
{
	public Task<List<Vzkaz>> GetVzkazyOdAsync(DateTime odExclusive, CancellationToken cancellationToken = default)
	{
		return _containers.Vzkazy.QueryToListAsync<Vzkaz>(
			new QueryDefinition("SELECT * FROM c WHERE c.datumVlozeni > @od AND NOT IS_DEFINED(c.deleted) ORDER BY c.datumVlozeni DESC")
				.WithParameter("@od", CosmosDateTimeConverter.ToCosmosString(odExclusive)),
			requestOptions: null,
			cancellationToken);
	}

	public async Task InsertAsync(Vzkaz vzkaz, CancellationToken cancellationToken = default)
	{
		vzkaz.Id ??= Guid.NewGuid().ToString();
		await _containers.Vzkazy.CreateItemAsync(vzkaz, new PartitionKey(vzkaz.Id), cancellationToken: cancellationToken);
	}
}
