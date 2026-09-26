using Havit.Services.TimeServices;
using KandaEu.Volejbal.DataLayer.Repositories;
using KandaEu.Volejbal.Model;
using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KandaEu.Volejbal.TestsForLocalDebugging.Services.Terminy;

[TestClass]
public class EnsureTerminyServiceTests : TestBase
{
	public TestContext TestContext { get; set; }

	/// <summary>
	/// Termíny se zakládají líně při čtení jejich seznamu, metoda tedy může běžet souběžně pro několik
	/// požadavků najednou. Test ověřuje, že souběh nezaloží duplicity ani neskončí výjimkou - proti sobě
	/// stojí unikátnost id dokumentu (= data termínu) a opakování pokusu v EnsureTerminyService.
	/// Vyžaduje reálný Cosmos (emulátor), unikátnost id nejde nasimulovat.
	/// </summary>
	[Ignore("Maže a znovu vytváří lokální databázi – spouštět jen ručně.")]
	[TestMethod]
	public async Task EnsureTerminyService_SoubeznaVolani_NezalozeDuplicitniTerminy()
	{
		// arrange
		const int PocetSoubeznychVolani = 8;
		IEnsureTerminyService ensureTerminyService = ServiceProvider.GetRequiredService<IEnsureTerminyService>();

		// act
		await Task.WhenAll(Enumerable.Range(0, PocetSoubeznychVolani)
			.Select(_ => ensureTerminyService.EnsureTerminyAsync(TestContext.CancellationToken)));

		// assert
		ITimeService timeService = ServiceProvider.GetRequiredService<ITimeService>();
		List<Termin> terminy = await ServiceProvider.GetRequiredService<ITerminRepository>()
			.GetBudouciTerminyAsync(timeService.GetCurrentDate(), TestContext.CancellationToken);

		Assert.HasCount(EnsureTerminyService.PozadovanyPocetBudoucichTerminu, terminy);
		Assert.AreAllDistinct(terminy.Select(termin => termin.Datum).ToList());
	}
}
