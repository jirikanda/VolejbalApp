using KandaEu.Volejbal.Contracts.Api;
using Refit;

namespace KandaEu.Volejbal.Contracts.Prihlasky;

public interface IPrihlaskaApi
{
	[Post("/" + ApiRoutes.TerminPrihlasit)]
	Task PrihlasitAsync(string terminId, string osobaId, CancellationToken cancellationToken = default);

	[Post("/" + ApiRoutes.TerminOdhlasit)]
	Task OdhlasitAsync(string terminId, string osobaId, CancellationToken cancellationToken = default);
}
