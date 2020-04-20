using System.ServiceModel;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.System
{
	[ServiceContract]
    public interface IDataSeedFacade
    {
        Task SeedDataProfile(string profileName); // TODO GRPC: Request?
    }
}