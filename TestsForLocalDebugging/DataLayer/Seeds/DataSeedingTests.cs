using System.Linq;
using Havit.Data.Patterns.DataSeeds;
using Havit.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KandaEu.Volejbal.DataLayer.Seeds.Core;
using Microsoft.Extensions.DependencyInjection;

namespace KandaEu.Volejbal.TestsForLocalDebugging.DataLayer.Seeds
{
	[TestClass]
	public class DataSeedingTests : TestBase
	{
		protected override bool UseLocalDb => true;
		protected override bool SeedData => false;

		//[TestMethod]
		[TestCategory("Explicit")]
		public void DataSeedRunner_SeedCoreProfile()
		{
			// arrange
			var dbContext = ServiceProvider.GetRequiredService<IDbContext>();
			var seedRunner = ServiceProvider.GetRequiredService<IDataSeedRunner>();
			dbContext.Database.EnsureDeleted();
			dbContext.Database.Migrate();

			// act
			seedRunner.SeedData<CoreProfile>();

			// assert
			// no exception
		}
	}
}
