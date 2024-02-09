using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Contracts.Osoby;

public interface IOsobaFacade
{
	Task VlozOsobuAsync(OsobaInputDto osobaInputDto, CancellationToken cancellationToken = default);
	Task SmazNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken = default);
	Task AktivujNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken = default);
	Task<OsobaListDto> GetNeaktivniOsobyAsync(CancellationToken cancellationToken = default);
	Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken = default);
}
