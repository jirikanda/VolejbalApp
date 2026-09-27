using KandaEu.Volejbal.Services.SkolniRok;

namespace KandaEu.Volejbal.Tests.Services.SkolniRok;

[TestClass]
public class SkolniRokServiceTests
{
	private readonly SkolniRokService _service = new SkolniRokService();

	[TestMethod]
	public void GetZacatek_HraniceJePrvniZari()
	{
		Assert.AreEqual(new DateTime(2025, 9, 1), _service.GetZacatek(new DateTime(2025, 9, 1)));
		Assert.AreEqual(new DateTime(2024, 9, 1), _service.GetZacatek(new DateTime(2025, 8, 31)));
		Assert.AreEqual(new DateTime(2025, 9, 1), _service.GetZacatek(new DateTime(2026, 1, 13)));
		Assert.AreEqual(new DateTime(2025, 9, 1), _service.GetZacatek(new DateTime(2026, 6, 30)));
		Assert.AreEqual(new DateTime(2026, 9, 1), _service.GetZacatek(new DateTime(2026, 9, 1)));
	}
}
