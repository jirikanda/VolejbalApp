using KandaEu.Volejbal.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KandaEu.Volejbal.Tests.Entity;

[TestClass]
public class VolejbalDbContextTests
{
	[TestMethod]
	public void VolejbalDbContext_CheckModelConventions()
	{
		// Arrange
		DbContextOptions<VolejbalDbContext> options = new DbContextOptionsBuilder<VolejbalDbContext>()
			.UseInMemoryDatabase(nameof(VolejbalDbContext))
			.Options;
		VolejbalDbContext dbContext = new VolejbalDbContext(options);

		// Act
		Havit.Data.EntityFrameworkCore.ModelValidation.ModelValidator modelValidator = new Havit.Data.EntityFrameworkCore.ModelValidation.ModelValidator();
		string errors = modelValidator.Validate(dbContext);

		// Assert
		if (!String.IsNullOrEmpty(errors))
		{
			Assert.Fail(errors);
		}
	}
}
