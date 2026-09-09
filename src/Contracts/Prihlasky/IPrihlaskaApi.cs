using KandaEu.Volejbal.Contracts.Api;
using Refit;

namespace KandaEu.Volejbal.Contracts.Prihlasky;

public interface IPrihlaskaApi
{
	[Post("/" + ApiRoutes.TerminPrihlasit)]
	Task PrihlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken = default);

	[Post("/" + ApiRoutes.TerminOdhlasit)]
	Task OdhlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken = default);
}
