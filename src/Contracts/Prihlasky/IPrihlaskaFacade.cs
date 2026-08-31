namespace KandaEu.Volejbal.Contracts.Prihlasky;

public interface IPrihlaskaFacade
{
	Task PrihlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken = default);
	Task OdhlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken = default);
}
