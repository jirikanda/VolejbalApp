using KandaEu.Volejbal.Services.Client.Platba;

namespace KandaEu.Volejbal.Tests.Platba;

[TestClass]
public class IbanBuilderTests
{
	[TestMethod]
	public void FromCzechAccountNumber_WithPrefix_ComputesIban()
	{
		Assert.AreEqual("CZ6508000000192000145399", IbanBuilder.FromCzechAccountNumber("19-2000145399/0800"));
	}

	[TestMethod]
	public void FromCzechAccountNumber_WithoutPrefix_PadsWithZeros()
	{
		Assert.AreEqual("CZ6655000000000310264001", IbanBuilder.FromCzechAccountNumber("310264001/5500"));
	}

	[TestMethod]
	public void FromCzechAccountNumber_IgnoresWhitespace()
	{
		Assert.AreEqual("CZ6508000000192000145399", IbanBuilder.FromCzechAccountNumber(" 19 - 2000145399 / 0800 "));
	}

	[TestMethod]
	public void FromCzechAccountNumber_InvalidFormat_Throws()
	{
		Assert.ThrowsExactly<FormatException>(() => IbanBuilder.FromCzechAccountNumber("CZ6508000000192000145399"));
	}
}
