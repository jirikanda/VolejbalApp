namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

public interface IEnsureTerminyService
{
	Task EnsureTerminyAsync(CancellationToken cancellationToken);
}
