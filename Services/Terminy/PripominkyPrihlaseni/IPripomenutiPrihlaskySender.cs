namespace KandaEu.Volejbal.Services.Terminy.PripominkyPrihlaseni;

public interface IPripomenutiPrihlaskySender
{
	Task SendPripominkaAsync(Osoba osoba, CancellationToken cancellationToken);
}