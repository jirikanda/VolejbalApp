using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.DataLayer.DataSources;
using KandaEu.Volejbal.Facades.Terminy.Dto;
using KandaEu.Volejbal.Facades.Terminy.Dto.Extensions;
using KandaEu.Volejbal.Model;
using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;
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
		private readonly IEnsureTerminyService ensureTerminyService;

		public TerminFacade(ITerminDataSource terminDataSource, IPrihlaskaDataSource prihlaskaDataSource, IOsobaDataSource osobaDataSource, ITimeService timeService, IUnitOfWork unitOfWork, IEnsureTerminyService ensureTerminyService)
		{
			this.terminDataSource = terminDataSource;
			this.prihlaskaDataSource = prihlaskaDataSource;
			this.osobaDataSource = osobaDataSource;
			this.timeService = timeService;
			this.unitOfWork = unitOfWork;
			this.ensureTerminyService = ensureTerminyService;
		}

		public TerminListDto GetTerminy()
		{
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
				.Where(osoba => osoba.Aktivni)
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
