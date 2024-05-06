namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial interface ITerminRepository
{
	Task<Termin> GetNextTerminStartingTommorowAsync(DateTime now, CancellationToken cancellationToken = default);
}
