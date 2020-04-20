using System.ServiceModel;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Prihlasky
{
	[ServiceContract]
	public interface IPrihlaskaFacade
	{
		Task Prihlasit(int terminId, int osobaId);// TODO GRPC: Request
		Task Odhlasit(int terminId, int osobaId);// TODO GRPC: Request
	}
}