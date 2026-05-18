using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;

namespace KandaEu.Volejbal.Facades.Nastenka;

[Service]
public class NastenkaFacade(
	IVzkazDataSource _vzkazDataSource,
	ITimeService _timeService,
	IUnitOfWork _unitOfWork,
	IOsobaRepository _osobaRepository) : INastenkaFacade
{
	public async Task<VzkazListDto> GetVzkazyAsync(CancellationToken cancellationToken)
	{
		DateTime today = _timeService.GetCurrentDate();
		DateTime prispevkyOd = today.AddDays(-14);
		DateTime currentWaveStart = GetCurrentWaveStart(today);

		VzkazListDto result = new VzkazListDto
		{
			Vzkazy = await _vzkazDataSource.Data
				.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetVzkazyAsync)))
				.Where(item => item.DatumVlozeni > prispevkyOd)
				.OrderByDescending(item => item.DatumVlozeni)
				.Select(vzkaz => new VzkazDto
				{
					Author = vzkaz.Autor.PrijmeniJmeno,
					Zprava = vzkaz.Zprava,
					DatumVlozeni = vzkaz.DatumVlozeni,
					IsObsolete = vzkaz.DatumVlozeni < currentWaveStart
				})
				.ToListAsync(cancellationToken)
		};

		return result;
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
		Osoba autor = await _osobaRepository.GetObjectAsync(vzkazInputDto.AutorId, cancellationToken);
		autor.ThrowIfDeleted();
		autor.ThrowIfNotAktivni();

		Vzkaz vzkaz = new Vzkaz
		{
			AutorId = vzkazInputDto.AutorId,
			Zprava = vzkazInputDto.Zprava,
			DatumVlozeni = _timeService.GetCurrentTime()
		};

		_unitOfWork.AddForInsert(vzkaz);
		await _unitOfWork.CommitAsync(cancellationToken);
	}
}
