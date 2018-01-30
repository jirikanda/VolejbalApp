using Castle.Windsor;
using Havit.Data.Patterns.DataSeeds;
using Havit.NewProjectTemplate.DataLayer.Seeds.Core;

namespace Havit.NewProjectTemplate.WebAPI.Tools
{
	public static class DataSeed
	{
		public static void Run(IWindsorContainer container)
		{
            IDataSeedRunner dataSeedRunner = container.Resolve<IDataSeedRunner>();
            dataSeedRunner.SeedData<CoreProfile>();
            container.Release(dataSeedRunner);
        }
	}
}
