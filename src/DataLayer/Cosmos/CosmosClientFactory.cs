using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace KandaEu.Volejbal.DataLayer.Cosmos;

/// <summary>
/// Sestavení <see cref="CosmosClient" />a. Klient je drahý (drží spojení, cache adres) a v aplikaci
/// žije jako singleton, viz <see cref="ServiceCollectionExtensions.AddDataLayerServices" />.
/// </summary>
public static class CosmosClientFactory
{
	public static CosmosClient Create(CosmosOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		options.Validate();

		CosmosClientOptions clientOptions = new CosmosClientOptions
		{
			ApplicationName = "VolejbalApp",
			// Gateway (HTTPS) místo Direct (TCP): instance Flex Consumption žije krátce a s 0,25 jádra
			// by si navazování direct kanálů nikdy neamortizovala. Gateway je navíc jediný režim,
			// který jde přes firewally beze změn.
			ConnectionMode = ConnectionMode.Gateway,
			// Zápisy nevrací tělo dokumentu - šetří to RU i přenos, ETag chodí v hlavičce tak jako tak.
			EnableContentResponseOnWrite = false,
			UseSystemTextJsonSerializerWithOptions = CreateSerializerOptions()
		};

		if (String.IsNullOrEmpty(options.Key))
		{
			// Produkce: managed identita Function App (datová role přiřazená v infra/main.bicep),
			// lokálně proti Azure účet z "az login". Klíč tak nikde neleží.
			return new CosmosClient(options.Endpoint, new DefaultAzureCredential(), clientOptions);
		}

		return new CosmosClient(options.Endpoint, options.Key, clientOptions);
	}

	/// <summary>
	/// Serializace dokumentů. Musí být System.Text.Json s camelCase: výchozí Newtonsoft s PascalCase
	/// by uložil "Id", zatímco Cosmos vyžaduje "id".
	/// </summary>
	/// <remarks>
	/// WhenWritingNull: null se do dokumentu nikdy nezapíše. Soft-delete dotazy se ptají
	/// <c>NOT IS_DEFINED(c.deleted)</c>; zapsaný null by vlastnost definoval a dotazy by tiše přestaly
	/// odpovídat.
	/// </remarks>
	public static JsonSerializerOptions CreateSerializerOptions()
	{
		JsonSerializerOptions serializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		serializerOptions.Converters.Add(new CosmosDateTimeConverter());

		return serializerOptions;
	}
}
