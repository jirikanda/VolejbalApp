using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Contracts.Osoby;

public interface IOsobaFacade
{
	Task VlozOsobuAsync(OsobaInputDto osobaInputDto, CancellationToken cancellationToken = default);
	Task SmazOsobuAsync(int osobaId, CancellationToken cancellationToken = default);
	Task AktivujOsobuAsync(int osobaId, CancellationToken cancellationToken = default);
	Task DeaktivujOsobuAsync(int osobaId, CancellationToken cancellationToken = default);
	Task<OsobaListDto> GetOsobyAsync(CancellationToken cancellationToken = default);
	Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken = default);
}
