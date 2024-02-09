using Havit.Data.EntityFrameworkCore;
using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using Microsoft.EntityFrameworkCore;

namespace KandaEu.Volejbal.Facades.Nastenka;

[Service]
public class NastenkaFacade : INastenkaFacade
{
	private readonly IVzkazDataSource vzkazDataSource;
	private readonly ITimeService timeService;
	private readonly IUnitOfWork unitOfWork;
	private readonly IOsobaRepository osobaRepository;

	public NastenkaFacade(IVzkazDataSource vzkazDataSource, ITimeService timeService, IUnitOfWork unitOfWork, IOsobaRepository osobaRepository)
	{
		this.vzkazDataSource = vzkazDataSource;
		this.timeService = timeService;
		this.unitOfWork = unitOfWork;
		this.osobaRepository = osobaRepository;
	}

	public async Task<VzkazListDto> GetVzkazyAsync(CancellationToken cancellationToken)
	{
		DateTime today = timeService.GetCurrentDate();
		DateTime prispevkyOd = today.AddDays(-14);

		VzkazListDto result = new VzkazListDto
		{
			Vzkazy = await vzkazDataSource.Data
				.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetVzkazyAsync)))
				.Where(item => item.DatumVlozeni > prispevkyOd)
				.OrderByDescending(item => item.DatumVlozeni)
				.Select(vzkaz => new VzkazDto
				{
					Author = vzkaz.Autor.PrijmeniJmeno,
					Zprava = vzkaz.Zprava,
					DatumVlozeni = vzkaz.DatumVlozeni
				})
				.ToListAsync(cancellationToken)
		};

		return result;
	}

	public async Task VlozVzkazAsync(VzkazInputDto vzkazInputDto, CancellationToken cancellationToken)
	{
		Osoba autor = await osobaRepository.GetObjectAsync(vzkazInputDto.AutorId, cancellationToken);
		autor.ThrowIfDeleted();
		autor.ThrowIfNotAktivni();

		Vzkaz vzkaz = new Vzkaz
		{
			AutorId = vzkazInputDto.AutorId,
			Zprava = vzkazInputDto.Zprava,
			DatumVlozeni = timeService.GetCurrentTime()
		};

		unitOfWork.AddForInsert(vzkaz);
		await unitOfWork.CommitAsync(cancellationToken);
	}
}
