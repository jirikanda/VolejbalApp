using KandaEu.Volejbal.Services.Client.Platba;

namespace KandaEu.Volejbal.Tests.Platba;

[TestClass]
public class SpaydBuilderTests
{
	[TestMethod]
	public void Build_ProducesSpaydWithoutDiacriticsAndWithoutSpacesInIban()
	{
		string spayd = SpaydBuilder.Build("cz65 0800 0000 1920 0014 5399", "Volejbal, termín 8.8.2222");

		Assert.AreEqual("SPD*1.0*ACC:CZ6508000000192000145399*CC:CZK*MSG:Volejbal, termin 8.8.2222", spayd);
	}

	[TestMethod]
	public void Build_WithAmount_WritesItInvariantWithTwoDecimals()
	{
		string spayd = SpaydBuilder.Build("CZ6508000000192000145399", "Volejbal", 120.5m);

		Assert.AreEqual("SPD*1.0*ACC:CZ6508000000192000145399*AM:120.50*CC:CZK*MSG:Volejbal", spayd);
	}

	[TestMethod]
	public void Build_WithNonPositiveAmount_Throws()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => SpaydBuilder.Build("CZ6508000000192000145399", "Volejbal", 0m));
	}

	[TestMethod]
	public void Build_EscapesAsteriskAndTruncatesMessage()
	{
		string spayd = SpaydBuilder.Build("CZ6508000000192000145399", "a*" + new string('b', 100));

		string message = spayd.Substring(spayd.IndexOf("*MSG:", StringComparison.Ordinal) + "*MSG:".Length);
		Assert.IsTrue(message.StartsWith("a%2A", StringComparison.Ordinal));
		Assert.IsFalse(message.Contains('*'));
	}
}
