using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;

namespace KandaEu.Volejbal.Facades.Nastenka;

[Service(ServiceType = typeof(INastenkaApi))]
public class NastenkaFacade(
	IVzkazRepository _vzkazRepository,
	IOsobaRepository _osobaRepository,
	TimeProvider _timeProvider) : INastenkaApi
{
	public async Task<VzkazListDto> GetVzkazyAsync(CancellationToken cancellationToken)
	{
		DateTime today = _timeProvider.GetLocalToday();
		DateTime prispevkyOd = today.AddDays(-14);
		DateTime currentWaveStart = GetCurrentWaveStart(today);

		List<Vzkaz> vzkazy = await _vzkazRepository.GetVzkazyOdAsync(prispevkyOd, cancellationToken);

		// Autoři se dohledávají podle id včetně smazaných - vzkaz smazaného hráče má zůstat podepsaný.
		// Chybějící autor je porušená integrita dat a repozitář ji hlásí výjimkou.
		List<Osoba> autori = await _osobaRepository.GetOsobyAsync(vzkazy.Select(vzkaz => vzkaz.AutorId).Distinct().ToList(), cancellationToken);
		Dictionary<string, Osoba> autoriPodleId = autori.ToDictionary(autor => autor.Id);

		return new VzkazListDto
		{
			Vzkazy = vzkazy
				.Select(vzkaz => new VzkazDto
				{
					Author = autoriPodleId[vzkaz.AutorId].PrijmeniJmeno,
					Zprava = vzkaz.Zprava,
					DatumVlozeni = vzkaz.DatumVlozeni,
					IsObsolete = vzkaz.DatumVlozeni < currentWaveStart
				})
				.ToList()
		};
	}

	/// <summary>
	/// Vrací začátek aktuální "vlny" zpráv = nejbližší předchozí (nebo dnešní) středa, 00:00.
	/// Vzkazy od této chvíle jsou aktuální, starší jsou obsolete.
	/// </summary>
	private static DateTime GetCurrentWaveStart(DateTime today)
	{
		int daysSinceWednesday = ((int)today.DayOfWeek - (int)DayOfWeek.Wednesday + 7) % 7;
		return today.Date.AddDays(-daysSinceWednesday);
	}

	public async Task VlozVzkazAsync(VzkazInputDto vzkazInputDto, CancellationToken cancellationToken)
	{
		Osoba autor = await _osobaRepository.GetOsobaAsync(vzkazInputDto.AutorId, cancellationToken);
		autor.ThrowIfDeleted();
		autor.ThrowIfNotAktivni();

		Vzkaz vzkaz = new Vzkaz
		{
			AutorId = autor.Id,
			Zprava = vzkazInputDto.Zprava,
			DatumVlozeni = _timeProvider.GetLocalDateTime()
		};

		await _vzkazRepository.InsertAsync(vzkaz, cancellationToken);
	}
}
