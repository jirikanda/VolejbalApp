using Havit.Data.Patterns.DataSeeds;
using Havit.NewProjectTemplate.DataLayer.Seeds.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Havit.NewProjectTemplate.TestsForLocalDebugging.DataLayer.Seeds
{
    [TestClass]
    public class DataSeedingTests : TestBase
    {        
        //[TestMethod]
        [TestCategory("Explicit")]
        public void DataSeedRunner_SeedCoreProfile()
        {
            // arrange
            var seedRunner = Container.Resolve<IDataSeedRunner>();

            // act
            seedRunner.SeedData<CoreProfile>();

            // clean up
            Container.Release(seedRunner);

            // assert
            // no exception
        }
	}
}
