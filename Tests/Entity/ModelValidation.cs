using System;
using System.Data.Entity;
using Havit.NewProjectTemplate.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Havit.NewProjectTemplate.Tests.Entity
{
	[TestClass]
	public class BrokerTrustMetodikyOnlineDbContextTest
	{
		[TestMethod]
		public void BrokerTrustMetodikyOnlineDbContext_CheckModelConventions()
		{
			// Arrange
			NewProjectTemplateDbContext dbContext = new NewProjectTemplateDbContext();
			Database.SetInitializer(new DropCreateDatabaseAlways<NewProjectTemplateDbContext>());
			dbContext.Database.Initialize(true);

			// Act
			Havit.Data.Entity.Validators.ModelValidator modelValidator = new Havit.Data.Entity.Validators.ModelValidator();
			string errors = modelValidator.Validate(dbContext);

			// Clean up
			dbContext.Database.Delete();

			// Assert
			if (!String.IsNullOrEmpty(errors))
			{
				Assert.Fail(errors);
			}
		}
	}
}
