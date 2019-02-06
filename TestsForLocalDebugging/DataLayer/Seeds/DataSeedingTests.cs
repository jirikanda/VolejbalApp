using System.Linq;
using Havit.Data.Patterns.DataSeeds;
using Havit.VolejbalApp.DataLayer.Seeds.Core;
using Havit.VolejbalApp.Model.Common;
using Havit.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Havit.VolejbalApp.TestsForLocalDebugging.DataLayer.Seeds
{
	[TestClass]
	public class DataSeedingTests : TestBase
	{
		//[TestMethod]
		[TestCategory("Explicit")]
		public void DataSeedRunner_SeedCoreProfile()
		{
			// arrange
			var dbContext = Container.Resolve<IDbContext>();
			var seedRunner = Container.Resolve<IDataSeedRunner>();
			dbContext.Database.EnsureDeleted();
			dbContext.Database.Migrate();

			// act
			seedRunner.SeedData<CoreProfile>();

			// clean up
			Container.Release(seedRunner);

			// assert
			// no exception
		}
	}
}
