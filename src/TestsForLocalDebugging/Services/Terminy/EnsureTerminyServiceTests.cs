using Havit.Services.TimeServices;
using KandaEu.Volejbal.DataLayer.DataSources;
using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KandaEu.Volejbal.TestsForLocalDebugging.Services.Terminy;

[TestClass]
public class EnsureTerminyServiceTests : TestBase
{
	protected override bool SeedData => false;

	public TestContext TestContext { get; set; }

	/// <summary>
	/// Termíny se zakládají líně při čtení jejich seznamu, metoda tedy může běžet souběžně pro několik
	/// požadavků najednou. Test ověřuje, že souběh nezaloží duplicity ani neskončí výjimkou - proti sobě
	/// stojí unikátní index UIDX_Termin_Datum_Deleted a opakování pokusu v EnsureTerminyService.
	/// Nelze spustit proti InMemory databázi, ta unikátní indexy nevynucuje.
	/// </summary>
	[Ignore("Maže a znovu vytváří lokální databázi – spouštět jen ručně.")]
	[TestMethod]
	public async Task EnsureTerminyService_SoubeznaVolani_NezalozeDuplicitniTerminy()
	{
		// arrange
		const int PocetSoubeznychVolani = 8;

		// act
		// Každé volání má vlastní scope, tedy i vlastní DbContext a UnitOfWork - jako by šlo o samostatné požadavky.
		await Task.WhenAll(Enumerable.Range(0, PocetSoubeznychVolani).Select(async _ =>
		{
			using IServiceScope scope = ServiceProvider.CreateScope();
			await scope.ServiceProvider.GetRequiredService<IEnsureTerminyService>().EnsureTerminyAsync(CancellationToken.None);
		}));

		// assert
		using IServiceScope assertScope = ServiceProvider.CreateScope();
		ITimeService timeService = assertScope.ServiceProvider.GetRequiredService<ITimeService>();
		List<DateTime> data = await assertScope.ServiceProvider.GetRequiredService<ITerminDataSource>().DataIncludingDeleted
			.Where(termin => termin.Datum.Date >= timeService.GetCurrentDate())
			.Select(termin => termin.Datum)
			.ToListAsync(TestContext.CancellationToken);

		Assert.HasCount(EnsureTerminyService.PozadovanyPocetBudoucichTerminu, data);
		Assert.AreAllDistinct(data);
	}
}
