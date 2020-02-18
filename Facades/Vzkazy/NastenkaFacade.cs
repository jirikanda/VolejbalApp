using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using KandaEu.Volejbal.DataLayer.DataSources;
using KandaEu.Volejbal.DataLayer.Repositories;
using KandaEu.Volejbal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KandaEu.Volejbal.Facades.Vzkazy
{
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

		public VzkazListDto GetVzkazy()
		{
			DateTime today = timeService.GetCurrentDate();
			DateTime prispevkyOd = today.AddDays(-14);

			VzkazListDto result = new VzkazListDto
			{
				Vzkazy = vzkazDataSource.Data
					.Where(item => item.DatumVlozeni > prispevkyOd)
					.OrderByDescending(item => item.DatumVlozeni)
					.Select(vzkaz => new VzkazDto
					{
						Author = vzkaz.Autor.PrijmeniJmeno,
						Zprava = vzkaz.Zprava,
						DatumVlozeni = vzkaz.DatumVlozeni
					}).ToList()
			};

			return result;
		}

		public void VlozVzkaz(VzkazInputDto vzkazInputDto)
		{
			Osoba autor = osobaRepository.GetObject(vzkazInputDto.AutorId);
			autor.ThrowIfDeleted();
			autor.ThrowIfNotAktivni();

			Vzkaz vzkaz = new Vzkaz
			{
				AutorId = vzkazInputDto.AutorId,
				Zprava = vzkazInputDto.Zprava,
				DatumVlozeni = timeService.GetCurrentTime()
			};

			unitOfWork.AddForInsert(vzkaz);
			unitOfWork.Commit();
		}
	}
}
