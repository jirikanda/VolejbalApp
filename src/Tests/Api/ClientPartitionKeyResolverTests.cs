using System.Net;
using KandaEu.Volejbal.Api.Infrastructure;

namespace KandaEu.Volejbal.Tests.Api;

/// <summary>
/// Určení klienta pro rate limiting z X-Forwarded-For - chyba tu buď sloučí všechny klienty do jednoho
/// čítače (a zablokuje aplikaci všem), nebo dovolí limit obejít podvrženou hlavičkou.
/// </summary>
[TestClass]
public class ClientPartitionKeyResolverTests
{
	[TestMethod]
	public void GetPartitionKey_AdresaSPortem_VraciAdresuBezPortu()
	{
		Assert.AreEqual("203.0.113.7", ClientPartitionKeyResolver.GetPartitionKey("203.0.113.7:51234", null));
	}

	[TestMethod]
	public void GetPartitionKey_PodvrzenaAdresaVlevo_VraciAdresuDoplnenouPlatformou()
	{
		Assert.AreEqual("203.0.113.7", ClientPartitionKeyResolver.GetPartitionKey("198.51.100.1, 203.0.113.7:51234", null));
	}

	[TestMethod]
	public void GetPartitionKey_InterniAdresaVpravo_Preskoci()
	{
		Assert.AreEqual("203.0.113.7", ClientPartitionKeyResolver.GetPartitionKey("203.0.113.7:51234, 10.0.0.4, 127.0.0.1", null));
	}

	[TestMethod]
	public void GetPartitionKey_IPv6_SeskupujePodlePrefixu64()
	{
		string key1 = ClientPartitionKeyResolver.GetPartitionKey("[2001:db8:1:2:aaaa::1]:443", null);
		string key2 = ClientPartitionKeyResolver.GetPartitionKey("2001:db8:1:2:bbbb::2", null);

		Assert.AreEqual("2001:db8:1:2::/64", key1);
		Assert.AreEqual(key1, key2);
	}

	[TestMethod]
	public void GetPartitionKey_IPv4MappedIPv6_VraciIPv4()
	{
		Assert.AreEqual("203.0.113.7", ClientPartitionKeyResolver.GetPartitionKey("[::ffff:203.0.113.7]:443", null));
	}

	[TestMethod]
	public void GetPartitionKey_BezHlavicky_PouzijeVerejnouRemoteIpAddress()
	{
		Assert.AreEqual("203.0.113.7", ClientPartitionKeyResolver.GetPartitionKey(null, IPAddress.Parse("203.0.113.7")));
	}

	[TestMethod]
	public void GetPartitionKey_JenInterniAdresy_VraciNull()
	{
		Assert.IsNull(ClientPartitionKeyResolver.GetPartitionKey("10.0.0.4, 100.64.1.1", IPAddress.Loopback));
	}

	[TestMethod]
	public void GetPartitionKey_NeplatnaHodnota_VraciNull()
	{
		Assert.IsNull(ClientPartitionKeyResolver.GetPartitionKey("unknown, garbage", null));
	}
}
