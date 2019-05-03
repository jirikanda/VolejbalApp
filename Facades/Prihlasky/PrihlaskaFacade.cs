using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.TimeServices;
using KandaEu.Volejbal.DataLayer.DataSources;
using KandaEu.Volejbal.Facades.Prihlasky.Dto;
using KandaEu.Volejbal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KandaEu.Volejbal.Facades.Prihlasky
{
	[Service]
	public class PrihlaskaFacade : IPrihlaskaFacade
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly ITimeService timeService;
		private readonly IPrihlaskaDataSource prihlaskaDataSource;

		public PrihlaskaFacade(IUnitOfWork unitOfWork, ITimeService timeService, IPrihlaskaDataSource prihlaskaDataSource)
		{
			this.unitOfWork = unitOfWork;
			this.timeService = timeService;
			this.prihlaskaDataSource = prihlaskaDataSource;
		}

		public void Prihlasit(int terminId, PrihlaskaOdhlaskaDto prihlaskaDto)
		{
			Prihlaska prihlaska = new Prihlaska
			{
				TerminId = terminId,
				OsobaId = prihlaskaDto.OsobaId,
				DatumPrihlaseni = timeService.GetCurrentTime(),
			};

			unitOfWork.AddForInsert(prihlaska);
			unitOfWork.Commit();
		}

		public void Odhlasit(int terminId, PrihlaskaOdhlaskaDto odhlaskaDto)
		{
			var prihlaska = prihlaskaDataSource.Data.Where(item => (item.TerminId == terminId) && (item.OsobaId == odhlaskaDto.OsobaId)).Single();

			unitOfWork.AddForDelete(prihlaska);
			unitOfWork.Commit();
		}
	}
}
