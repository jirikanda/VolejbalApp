namespace KandaEu.Volejbal.Contracts.System;

public interface IDataSeedFacade
{
	Task SeedDataProfileAsync(string profileName, CancellationToken cancellationToken = default);
}
