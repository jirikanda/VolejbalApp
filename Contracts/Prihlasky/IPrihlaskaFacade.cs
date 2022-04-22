using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Prihlasky;

public interface IPrihlaskaFacade
{
    Task Prihlasit(int terminId, int osobaId);
    Task Odhlasit(int terminId, int osobaId);
}
