namespace KandaEu.Volejbal.Contracts.Prihlasky
{
	public interface IPrihlaskaFacade
	{
		void Prihlasit(int terminId, int osobaId);
		void Odhlasit(int terminId, int osobaId);
	}
}