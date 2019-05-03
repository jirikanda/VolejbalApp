using KandaEu.Volejbal.Facades.Prihlasky.Dto;

namespace KandaEu.Volejbal.Facades.Prihlasky
{
	public interface IPrihlaskaFacade
	{
		void Prihlasit(int terminId, PrihlaskaOdhlaskaDto prihlaskaDto);
		void Odhlasit(int terminId, PrihlaskaOdhlaskaDto odhlaskaDto);
	}
}