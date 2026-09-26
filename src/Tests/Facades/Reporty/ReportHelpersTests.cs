using KandaEu.Volejbal.Facades.Reporty;

namespace KandaEu.Volejbal.Tests.Facades.Reporty;

[TestClass]
public class ReportHelpersTests
{
	[TestMethod]
	public void GetZacatekSkolnihoRoku_HraniceJePrvniZari()
	{
		Assert.AreEqual(new DateTime(2025, 9, 1), ReportHelpers.GetZacatekSkolnihoRoku(new DateTime(2025, 9, 1)));
		Assert.AreEqual(new DateTime(2024, 9, 1), ReportHelpers.GetZacatekSkolnihoRoku(new DateTime(2025, 8, 31)));
		Assert.AreEqual(new DateTime(2025, 9, 1), ReportHelpers.GetZacatekSkolnihoRoku(new DateTime(2026, 1, 13)));
		Assert.AreEqual(new DateTime(2025, 9, 1), ReportHelpers.GetZacatekSkolnihoRoku(new DateTime(2026, 6, 30)));
		Assert.AreEqual(new DateTime(2026, 9, 1), ReportHelpers.GetZacatekSkolnihoRoku(new DateTime(2026, 9, 1)));
	}
}
