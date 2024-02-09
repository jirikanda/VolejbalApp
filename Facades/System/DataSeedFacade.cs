using Havit;
using Havit.Data.Patterns.DataSeeds;
using KandaEu.Volejbal.Contracts.System;
using KandaEu.Volejbal.DataLayer.Seeds.Core;

namespace KandaEu.Volejbal.Facades.System;

/// <summary>
/// Fasáda k seedování dat.
/// </summary>
[Service]
public class DataSeedFacade : IDataSeedFacade
{
	private readonly IDataSeedRunner dataSeedRunner;

	public DataSeedFacade(IDataSeedRunner dataSeedRunner)
	{
		this.dataSeedRunner = dataSeedRunner;
	}

	/// <summary>
	/// Provede seedování dat daného profilu.
	/// Pokud jde produkční prostředí a profil není pro produkční prostředí povolen, vrací BadRequest.
	/// </summary>        
	public async Task SeedDataProfileAsync(string profileName, CancellationToken cancellationToken)
	{
		string typeName = profileName + "Profile";
		Type type = typeof(CoreProfile).Assembly.GetTypes().FirstOrDefault(item => String.Equals(item.Name, typeName, StringComparison.InvariantCultureIgnoreCase));

		if (type == null)
		{
			throw new OperationFailedException($"Profil {profileName} nebyl nalezen.");
		}

		await dataSeedRunner.SeedDataAsync(type, cancellationToken: cancellationToken);
	}
}
