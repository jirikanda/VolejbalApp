using KandaEu.Volejbal.Contracts.System.Dto;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.System
{
    public interface IDataSeedFacade
    {
        Task SeedDataProfile(SeedDataProfileRequest seedDataProfileRequest);
    }
}