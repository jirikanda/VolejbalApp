using KandaEu.Volejbal.Contracts.System.Dto;
using System.ServiceModel;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.System
{
    [ServiceContract]
    public interface IDataSeedFacade
    {
        Task SeedDataProfile(SeedDataProfileRequest seedDataProfileRequest);
    }
}