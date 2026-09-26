using Microsoft.Azure.Cosmos;

namespace KandaEu.Volejbal.DataLayer.Cosmos;

public static class CosmosContainerExtensions
{
	/// <summary>
	/// Načte všechny výsledky dotazu (projde všechny stránky).
	/// </summary>
	public static async Task<List<T>> QueryToListAsync<T>(this Container container, QueryDefinition query, QueryRequestOptions requestOptions, CancellationToken cancellationToken)
	{
		List<T> result = new List<T>();

		using FeedIterator<T> iterator = container.GetItemQueryIterator<T>(query, requestOptions: requestOptions);
		while (iterator.HasMoreResults)
		{
			FeedResponse<T> page = await iterator.ReadNextAsync(cancellationToken);
			result.AddRange(page);
		}

		return result;
	}
}
