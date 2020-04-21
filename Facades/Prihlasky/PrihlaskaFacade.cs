using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Prihlasky.Dto;
using KandaEu.Volejbal.DataLayer.DataSources;
using KandaEu.Volejbal.DataLayer.Repositories;
using KandaEu.Volejbal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Facades.Prihlasky
{
	[Service]
	public class PrihlaskaFacade : IPrihlaskaFacade
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly ITimeService timeService;
		private readonly IPrihlaskaRepository prihlaskaRepository;

		private static object _lock = new object();

		public PrihlaskaFacade(IUnitOfWork unitOfWork, ITimeService timeService, IPrihlaskaRepository prihlaskaRepository)
		{
			this.unitOfWork = unitOfWork;
			this.timeService = timeService;
			this.prihlaskaRepository = prihlaskaRepository;
		}

		public async Task Prihlasit(PrihlasitOdhlasitRequest prihlasitRequest)
		{
			// TODO: lock (_lock)
			{
				if (await prihlaskaRepository.GetPrihlaska(prihlasitRequest.TerminId, prihlasitRequest.OsobaId) == null)
				{
					Prihlaska prihlaska = new Prihlaska
					{
						TerminId = prihlasitRequest.TerminId,
						OsobaId = prihlasitRequest.OsobaId,
						DatumPrihlaseni = timeService.GetCurrentTime(),
					};

					unitOfWork.AddForInsert(prihlaska);
					await unitOfWork.CommitAsync();
				}
			}
		}

		public async Task Odhlasit(PrihlasitOdhlasitRequest odhlasitRequest)
		{
			// TODO: lock (_lock)
			{
				Prihlaska prihlaska = await prihlaskaRepository.GetPrihlaska(odhlasitRequest.TerminId, odhlasitRequest.OsobaId);
				if (prihlaska != null)
				{
					unitOfWork.AddForDelete(prihlaska);
					await unitOfWork.CommitAsync();
				}
			}
		}
	}
}
