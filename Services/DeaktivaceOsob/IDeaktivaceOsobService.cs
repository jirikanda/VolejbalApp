namespace KandaEu.Volejbal.Services.DeaktivaceOsob;

public interface IDeaktivaceOsobService
{
	public Task DeaktivujOsobyAsync(CancellationToken cancellationToken = default);
}
