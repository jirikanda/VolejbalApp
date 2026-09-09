using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using Refit;

namespace KandaEu.Volejbal.Contracts.Osoby;

public interface IOsobaApi
{
	[Post("/" + ApiRoutes.Osoby)]
	Task VlozOsobuAsync([Body] OsobaInputDto osobaInputDto, CancellationToken cancellationToken = default);

	[Delete("/" + ApiRoutes.Osoba)]
	Task SmazOsobuAsync(int osobaId, CancellationToken cancellationToken = default);

	[Post("/" + ApiRoutes.OsobaAktivovat)]
	Task AktivujOsobuAsync(int osobaId, CancellationToken cancellationToken = default);

	[Post("/" + ApiRoutes.OsobaDeaktivovat)]
	Task DeaktivujOsobuAsync(int osobaId, CancellationToken cancellationToken = default);

	[Get("/" + ApiRoutes.Osoby)]
	Task<OsobaListDto> GetOsobyAsync(CancellationToken cancellationToken = default);

	[Get("/" + ApiRoutes.OsobyAktivni)]
	Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken = default);
}
