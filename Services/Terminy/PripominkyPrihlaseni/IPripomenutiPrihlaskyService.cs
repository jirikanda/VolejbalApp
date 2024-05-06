namespace KandaEu.Volejbal.Services.Terminy.PripominkyPrihlaseni;

public interface IPripomenutiPrihlaskyService
{
	Task SendPripominkyAsync(CancellationToken cancellationToken);
}