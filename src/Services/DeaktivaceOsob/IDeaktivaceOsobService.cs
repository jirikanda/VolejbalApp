namespace KandaEu.Volejbal.Services.DeaktivaceOsob;

public interface IDeaktivaceOsobService
{
	Task DeaktivujOsobyAsync(CancellationToken cancellationToken = default);
}
