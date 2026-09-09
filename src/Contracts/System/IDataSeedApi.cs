using KandaEu.Volejbal.Contracts.Api;
using Refit;

namespace KandaEu.Volejbal.Contracts.System;

public interface IDataSeedApi
{
	[Post("/" + ApiRoutes.SystemSeed)]
	Task SeedDataProfileAsync(string profileName, CancellationToken cancellationToken = default);
}
