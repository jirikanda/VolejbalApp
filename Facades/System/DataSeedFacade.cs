using System;
using System.Linq;
using System.Threading.Tasks;
using Havit.Data.Patterns.DataSeeds;
using Havit.Extensions.DependencyInjection.Abstractions;
using KandaEu.Volejbal.Contracts.System;
using KandaEu.Volejbal.DataLayer.Seeds.Core;
using KandaEu.Volejbal.Services.Infrastructure;

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
    public Task SeedDataProfile(string profileName)
    {
        string typeName = profileName + "Profile";
        Type type = typeof(CoreProfile).Assembly.GetTypes().FirstOrDefault(item => String.Equals(item.Name, typeName, StringComparison.InvariantCultureIgnoreCase));

        if (type == null)
        {
            throw new OperationFailedException($"Profil {profileName} nebyl nalezen.");
        }

        dataSeedRunner.SeedData(type);

        return Task.CompletedTask;
    }
}
