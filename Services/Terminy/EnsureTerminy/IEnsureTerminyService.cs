namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

public interface IEnsureTerminyService
{
	public Task EnsureTerminyAsync(CancellationToken cancellationToken);
}
