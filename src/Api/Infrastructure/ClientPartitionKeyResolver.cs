using System.Net;
using System.Net.Sockets;

namespace KandaEu.Volejbal.Api.Infrastructure;

/// <summary>
/// Určuje, za jakého klienta se request počítá do rate limitingu.
/// </summary>
/// <remarks>
/// Worker za frontendem platformy a za hostem Functions nevidí adresu klienta v RemoteIpAddress, jen
/// v hlavičce X-Forwarded-For. Hlavičku může klient sám vyplnit, platforma ale skutečnou adresu doplňuje
/// za ni - proto se bere nejpravější veřejná adresa, ne první. Interní adresy (loopback, privátní rozsahy)
/// se přeskakují, kdyby do hlavičky přidal záznam některý z interních proxy.
/// IPv6 se seskupuje podle prefixu /64: koncové zařízení má celý /64 a adresu v něm běžně střídá.
/// </remarks>
public static class ClientPartitionKeyResolver
{
	/// <summary>
	/// Vrací klíč klienta, nebo null, pokud se veřejnou adresu klienta zjistit nepodařilo.
	/// </summary>
	public static string GetPartitionKey(string forwardedFor, IPAddress remoteIpAddress)
	{
		IPAddress address = GetRightmostPublicAddress(forwardedFor);
		if ((address == null) && (remoteIpAddress != null) && IsPublic(Normalize(remoteIpAddress)))
		{
			address = Normalize(remoteIpAddress);
		}

		if (address == null)
		{
			return null;
		}

		if (address.AddressFamily == AddressFamily.InterNetworkV6)
		{
			byte[] bytes = address.GetAddressBytes();
			Array.Clear(bytes, 8, 8);
			return new IPAddress(bytes).ToString() + "/64";
		}

		return address.ToString();
	}

	private static IPAddress GetRightmostPublicAddress(string forwardedFor)
	{
		if (String.IsNullOrWhiteSpace(forwardedFor))
		{
			return null;
		}

		string[] entries = forwardedFor.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		for (int i = entries.Length - 1; i >= 0; i--)
		{
			// IPEndPoint.TryParse zvládá "1.2.3.4", "1.2.3.4:5678", "[2001:db8::1]:443" i "2001:db8::1".
			if (IPEndPoint.TryParse(entries[i], out IPEndPoint endPoint))
			{
				IPAddress address = Normalize(endPoint.Address);
				if (IsPublic(address))
				{
					return address;
				}
			}
		}

		return null;
	}

	private static IPAddress Normalize(IPAddress address)
	{
		return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
	}

	private static bool IsPublic(IPAddress address)
	{
		if (IPAddress.IsLoopback(address))
		{
			return false;
		}

		if (address.AddressFamily == AddressFamily.InterNetwork)
		{
			byte[] bytes = address.GetAddressBytes();
			return !((bytes[0] == 0)
				|| (bytes[0] == 10)
				|| ((bytes[0] == 100) && ((bytes[1] & 0xC0) == 64)) // 100.64.0.0/10 (CGNAT, interní sítě platformy)
				|| ((bytes[0] == 169) && (bytes[1] == 254))
				|| ((bytes[0] == 172) && ((bytes[1] & 0xF0) == 16))
				|| ((bytes[0] == 192) && (bytes[1] == 168)));
		}

		if (address.AddressFamily == AddressFamily.InterNetworkV6)
		{
			return !(address.IsIPv6LinkLocal
				|| address.IsIPv6SiteLocal
				|| address.IsIPv6UniqueLocal
				|| address.Equals(IPAddress.IPv6None));
		}

		return false;
	}
}
