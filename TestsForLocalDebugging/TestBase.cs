using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Havit.NewProjectTemplate.WindsorInstallers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Havit.NewProjectTemplate.TestsForLocalDebugging
{
    /// <summary>
    /// Bázový třída pro testy.
    /// Zpřístupňuje nakonfigurovaný DI container a transparentně zajišťuje scope.
    /// </summary>
    public class TestBase
    {
		private IDisposable scope;

		protected IWindsorContainer Container { get; private set; }
		
		[TestInitialize]
		public virtual void TestInitialize()
		{
			IWindsorContainer container = new WindsorContainer();
			container.ConfigureForTests();
			scope = container.BeginScope();

			this.Container = container;
		}

		[TestCleanup]
		public virtual void TestCleanUp()
		{
			this.Container.Dispose();
			this.Container = null;
			scope.Dispose();
		}
    }
}
