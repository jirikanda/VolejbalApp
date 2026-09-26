using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

namespace KandaEu.Volejbal.Tests.Services.Terminy;

/// <summary>
/// Výpočet dat termínů k založení - služba bez závislostí, takže se dá ověřit bez databáze i DI.
/// Data v testech jsou úterky, viz komentáře.
/// </summary>
[TestClass]
public class TerminDatumGeneratorServiceTests
{
	private readonly TerminDatumGeneratorService _service = new TerminDatumGeneratorService();

	[TestMethod]
	public void GetDatumyKZalozeni_ZadnyTermin_ZacinaPrvnimUterymOdeDneska()
	{
		// 2026-02-04 je středa, nejbližší úterý je 2026-02-10.
		List<DateTime> datumy = _service.GetDatumyKZalozeni(new DateTime(2026, 2, 4), posledniDatum: null, pocetBudoucichTerminu: 0, pozadovanyPocet: 3);

		Assert.AreSequenceEqual(new[] { new DateTime(2026, 2, 10), new DateTime(2026, 2, 17), new DateTime(2026, 2, 24) }, datumy);
	}

	[TestMethod]
	public void GetDatumyKZalozeni_DnesJeUtery_PrvniTerminJeDnes()
	{
		List<DateTime> datumy = _service.GetDatumyKZalozeni(new DateTime(2026, 2, 10), posledniDatum: null, pocetBudoucichTerminu: 0, pozadovanyPocet: 1);

		Assert.AreSequenceEqual(new[] { new DateTime(2026, 2, 10) }, datumy);
	}

	[TestMethod]
	public void GetDatumyKZalozeni_DostatekTerminu_NicNezaklada()
	{
		List<DateTime> datumy = _service.GetDatumyKZalozeni(new DateTime(2026, 2, 4), new DateTime(2026, 2, 17), pocetBudoucichTerminu: 3, pozadovanyPocet: 3);

		Assert.IsEmpty(datumy);
	}

	[TestMethod]
	public void GetDatumyKZalozeni_NavazujeNaPosledniTermin_StejnyDenNasledujiciTydny()
	{
		// Poslední termín 2026-02-10 (úterý) je v budoucnosti a je jediný - doplní se dva další.
		List<DateTime> datumy = _service.GetDatumyKZalozeni(new DateTime(2026, 2, 4), new DateTime(2026, 2, 10), pocetBudoucichTerminu: 1, pozadovanyPocet: 3);

		Assert.AreSequenceEqual(new[] { new DateTime(2026, 2, 17), new DateTime(2026, 2, 24) }, datumy);
	}

	[TestMethod]
	public void GetDatumyKZalozeni_PosledniTerminDavnoVMinulosti_PosuneSeDoBudoucnosti()
	{
		// Poslední termín 2026-01-13 (úterý), dnes 2026-02-04 (středa): +7 dní opakovaně až k prvnímu úterý >= dnes.
		List<DateTime> datumy = _service.GetDatumyKZalozeni(new DateTime(2026, 2, 4), new DateTime(2026, 1, 13), pocetBudoucichTerminu: 0, pozadovanyPocet: 1);

		Assert.AreSequenceEqual(new[] { new DateTime(2026, 2, 10) }, datumy);
	}

	[TestMethod]
	public void GetDatumyKZalozeni_PreskakujeLetniPrazdniny()
	{
		// Poslední termín 2026-06-23 (úterý): 30. 6. ještě ano, pak celý červenec a srpen ne, dál 1. 9. 2026 (úterý).
		List<DateTime> datumy = _service.GetDatumyKZalozeni(new DateTime(2026, 6, 20), new DateTime(2026, 6, 23), pocetBudoucichTerminu: 1, pozadovanyPocet: 3);

		Assert.AreSequenceEqual(new[] { new DateTime(2026, 6, 30), new DateTime(2026, 9, 1) }, datumy);
	}

	[TestMethod]
	public void GetDatumyKZalozeni_PreskakujeVanocniPrazdniny()
	{
		// Poslední termín 2025-12-23 (úterý): 30. 12. spadá do vánočních prázdnin, další je 6. 1. 2026.
		List<DateTime> datumy = _service.GetDatumyKZalozeni(new DateTime(2025, 12, 20), new DateTime(2025, 12, 23), pocetBudoucichTerminu: 1, pozadovanyPocet: 2);

		Assert.AreSequenceEqual(new[] { new DateTime(2026, 1, 6) }, datumy);
	}

	[TestMethod]
	public void GetDatumyKZalozeni_PreskakujeStatniSvatky()
	{
		// Poslední termín 2025-10-21 (úterý): 28. 10. je státní svátek, další je 4. 11.; 17. 11. 2026 (úterý) je svátek také.
		List<DateTime> datumy = _service.GetDatumyKZalozeni(new DateTime(2025, 10, 20), new DateTime(2025, 10, 21), pocetBudoucichTerminu: 1, pozadovanyPocet: 2);
		Assert.AreSequenceEqual(new[] { new DateTime(2025, 11, 4) }, datumy);

		datumy = _service.GetDatumyKZalozeni(new DateTime(2026, 11, 9), new DateTime(2026, 11, 10), pocetBudoucichTerminu: 1, pozadovanyPocet: 2);
		Assert.AreSequenceEqual(new[] { new DateTime(2026, 11, 24) }, datumy);
	}

	[TestMethod]
	public void GetDatumyKZalozeni_IgnorujeCasovouSlozku()
	{
		List<DateTime> datumy = _service.GetDatumyKZalozeni(new DateTime(2026, 2, 4, 18, 30, 0), new DateTime(2026, 2, 10, 9, 15, 0), pocetBudoucichTerminu: 1, pozadovanyPocet: 2);

		Assert.AreSequenceEqual(new[] { new DateTime(2026, 2, 17) }, datumy);
	}
}
