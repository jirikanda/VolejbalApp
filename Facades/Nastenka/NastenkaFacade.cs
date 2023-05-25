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

	public async Task<VzkazListDto> GetVzkazy()
	{
		DateTime today = timeService.GetCurrentDate();
		DateTime prispevkyOd = today.AddDays(-14);

		VzkazListDto result = new VzkazListDto
		{
			Vzkazy = await vzkazDataSource.Data
				.Where(item => item.DatumVlozeni > prispevkyOd)
				.OrderByDescending(item => item.DatumVlozeni)
				.Select(vzkaz => new VzkazDto
				{
					Author = vzkaz.Autor.PrijmeniJmeno,
					Zprava = vzkaz.Zprava,
					DatumVlozeni = vzkaz.DatumVlozeni
				}).ToListAsync()
		};

		return result;
	}

	public async Task VlozVzkaz(VzkazInputDto vzkazInputDto)
	{
		Osoba autor = await osobaRepository.GetObjectAsync(vzkazInputDto.AutorId);
		autor.ThrowIfDeleted();
		autor.ThrowIfNotAktivni();

		Vzkaz vzkaz = new Vzkaz
		{
			AutorId = vzkazInputDto.AutorId,
			Zprava = vzkazInputDto.Zprava,
			DatumVlozeni = timeService.GetCurrentTime()
		};

		unitOfWork.AddForInsert(vzkaz);
		await unitOfWork.CommitAsync();
	}
}
