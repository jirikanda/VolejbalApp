namespace KandaEu.Volejbal.Services.Jobs;

public interface IRunnableJob
{
	Task ExecuteAsync(CancellationToken cancellationToken = default);
}
