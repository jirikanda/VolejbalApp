using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.DataLayer.DataSources;
using KandaEu.Volejbal.Facades.Terminy.Dto;
using KandaEu.Volejbal.Facades.Terminy.Dto.Extensions;
using KandaEu.Volejbal.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KandaEu.Volejbal.Facades.Terminy
{
	[Service]
	public class TerminFacade : ITerminFacade
	{
		private readonly ITerminDataSource terminDataSource;
		private readonly IPrihlaskaDataSource prihlaskaDataSource;
		private readonly IOsobaDataSource osobaDataSource;
		private readonly ITimeService timeService;
		private readonly IUnitOfWork unitOfWork;

		public TerminFacade(ITerminDataSource terminDataSource, IPrihlaskaDataSource prihlaskaDataSource, IOsobaDataSource osobaDataSource, ITimeService timeService, IUnitOfWork unitOfWork)
		{
			this.terminDataSource = terminDataSource;
			this.prihlaskaDataSource = prihlaskaDataSource;
			this.osobaDataSource = osobaDataSource;
			this.timeService = timeService;
			this.unitOfWork = unitOfWork;
		}

		public TerminListDto GetTerminy()
		{
			EnsureTerminy();

			var terminy = terminDataSource.Data
				.Where(termin => termin.Datum.Date >= timeService.GetCurrentDate())
				.Select(item => new TerminDto
				{
					Id = item.Id,
					Datum = item.Datum
				}).ToList();
			
			return new TerminListDto
			{
				Terminy = terminy
			};
		}

		private void EnsureTerminy()
		{
			int budouciTerminyPocet = terminDataSource.Data.Where(termin => termin.Datum.Date >= timeService.GetCurrentDate()).Count();

			if (budouciTerminyPocet < 3)
			{
				DateTime posledniDatum = terminDataSource.DataWithDeleted.OrderByDescending(item => item.Datum).Select(item => item.Datum).FirstOrDefault();
				if (posledniDatum < timeService.GetCurrentDate())
				{
					posledniDatum = timeService.GetCurrentDate();
				}

				DateTime datum = posledniDatum.AddDays(1);
				while (datum.DayOfWeek != DayOfWeek.Tuesday) // TODO: Parametrizovat?
				{
					datum = datum.AddDays(1);
				}

				for (int i = budouciTerminyPocet; i < 3; i++)
				{
					Termin termin = new Termin { Datum = datum };
					unitOfWork.AddForInsert(termin);
					datum = datum.AddDays(7);
				}

				unitOfWork.Commit();
			}
		}

		public TerminDetailDto GetDetailTerminu(int terminId)
		{
			List<Prihlaska> prihlasky = prihlaskaDataSource.Data
				.Where(prihlaska => prihlaska.TerminId == terminId)
				.Include(prihlaska => prihlaska.Osoba)
				.OrderBy(prihlaska => prihlaska.DatumPrihlaseni)
				.ToList();

			List<Osoba> prihlaseni = prihlasky
				.Select(item => item.Osoba)
				.ToList();

			List<Osoba> neprihlaseni = osobaDataSource.Data
				.ToList()
				.Except(prihlaseni /* in memory */)
				.OrderBy(item => item.PrijmeniJmeno)
				.ToList();

			return new TerminDetailDto
			{
				Prihlaseni = prihlasky.Select(prihlaska => prihlaska.ToOsobaDto()).ToList(),
				Neprihlaseni = neprihlaseni.Select(osoba => osoba.ToOsobaDto()).ToList()
			};
		}

	}
}
