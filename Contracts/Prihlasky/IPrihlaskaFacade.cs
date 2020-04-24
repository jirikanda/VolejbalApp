using KandaEu.Volejbal.Contracts.Prihlasky.Dto;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Prihlasky
{
	public interface IPrihlaskaFacade
	{
		Task Prihlasit(PrihlasitOdhlasitRequest prihlasitRequest);
		Task Odhlasit(PrihlasitOdhlasitRequest odhlasitRequest);
	}
}