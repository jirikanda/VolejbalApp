using KandaEu.Volejbal.Contracts.Prihlasky.Dto;
using System.ServiceModel;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Prihlasky
{
	[ServiceContract]
	public interface IPrihlaskaFacade
	{
		Task Prihlasit(PrihlasitOdhlasitRequest prihlasitRequest);
		Task Odhlasit(PrihlasitOdhlasitRequest odhlasitRequest);
	}
}