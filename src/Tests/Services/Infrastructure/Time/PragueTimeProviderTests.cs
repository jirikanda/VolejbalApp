using KandaEu.Volejbal.Services.Infrastructure.Time;

namespace KandaEu.Volejbal.Tests.Services.Infrastructure.Time;

/// <summary>
/// Ověřuje to podstatné: v procesu běžícím v UTC dá provider pražské datum, a to i přes půlnoc.
/// </summary>
[TestClass]
public class PragueTimeProviderTests
{
	[TestMethod]
	public void GetLocalToday_PredPulnociVPraze_VraciNasledujiciDenOprotiUtc()
	{
		// 30. 6. 2026 22:30 UTC = 1. 7. 2026 0:30 SELČ.
		PevnyPragueTimeProvider timeProvider = new PevnyPragueTimeProvider(new DateTimeOffset(2026, 6, 30, 22, 30, 0, TimeSpan.Zero));

		Assert.AreEqual(new DateTime(2026, 7, 1), timeProvider.GetLocalToday());
		Assert.AreEqual(new DateTime(2026, 7, 1, 0, 30, 0), timeProvider.GetLocalDateTime());
		Assert.AreEqual(DateTimeKind.Unspecified, timeProvider.GetLocalDateTime().Kind);
	}

	[TestMethod]
	public void GetLocalDateTime_VZime_PouzivaStandardniCas()
	{
		// 13. 1. 2026 17:00 UTC = 18:00 SEČ.
		PevnyPragueTimeProvider timeProvider = new PevnyPragueTimeProvider(new DateTimeOffset(2026, 1, 13, 17, 0, 0, TimeSpan.Zero));

		Assert.AreEqual(new DateTime(2026, 1, 13, 18, 0, 0), timeProvider.GetLocalDateTime());
	}

	/// <summary>
	/// Provider s pevným UTC okamžikem - zóna zůstává pražská z <see cref="PragueTimeProvider" />.
	/// </summary>
	private sealed class PevnyPragueTimeProvider(DateTimeOffset _utcNow) : PragueTimeProvider
	{
		public override DateTimeOffset GetUtcNow()
		{
			return _utcNow;
		}
	}
}
