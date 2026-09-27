namespace KandaEu.Volejbal.DataLayer.Repositories;

public interface IVzkazRepository
{
	/// <summary>
	/// Nesmazané vzkazy vložené po zadaném datu, od nejnovějšího.
	/// </summary>
	Task<List<Vzkaz>> GetVzkazyOdAsync(DateTime odExclusive, CancellationToken cancellationToken = default);

	/// <summary>
	/// Vloží vzkaz a přidělí mu id (Cosmos nemá sekvence, id je GUID).
	/// </summary>
	Task InsertAsync(Vzkaz vzkaz, CancellationToken cancellationToken = default);
}
