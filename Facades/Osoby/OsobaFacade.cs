using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.DataLayer.DataSources;
using KandaEu.Volejbal.DataLayer.Repositories;
using KandaEu.Volejbal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KandaEu.Volejbal.Facades.Osoby
{
	[Service]
	public class OsobaFacade : IOsobaFacade
	{
		private readonly IOsobaRepository osobaRepository;
		private readonly IOsobaDataSource osobaDataSource;
		private readonly IUnitOfWork unitOfWork;

		public OsobaFacade(IOsobaRepository osobaRepository, IOsobaDataSource osobaDataSource, IUnitOfWork unitOfWork)
		{
			this.osobaRepository = osobaRepository;
			this.osobaDataSource = osobaDataSource;
			this.unitOfWork = unitOfWork;
		}

		public void VlozOsobu(OsobaInputDto osobaInputDto)
		{
			Osoba osoba = new Osoba
			{
				Jmeno = osobaInputDto.Jmeno,
				Prijmeni = osobaInputDto.Prijmeni,
				Email = osobaInputDto.Email
			};

			unitOfWork.AddForInsert(osoba);
			unitOfWork.Commit();
		}

		public void AktivujNeaktivniOsobu(int osobaId)
		{
			Osoba osoba = osobaRepository.GetObject(osobaId);

			CheckNeaktivniNesmazana(osoba);
			
			osoba.Aktivni = true;

			unitOfWork.AddForUpdate(osoba);
			unitOfWork.Commit();
		}

		public void SmazNeaktivniOsobu(int osobaId)
		{
			Osoba osoba = osobaRepository.GetObject(osobaId);

			unitOfWork.AddForDelete(osoba);
			unitOfWork.Commit();
		}

		private void CheckNeaktivniNesmazana(Osoba osoba)
		{
			osoba.ThrowIfDeleted();
			osoba.ThrowIfAktivni();
		}

		public OsobaListDto GetNeaktivniOsoby()
		{
			var result = new OsobaListDto
			{
				Osoby = osobaDataSource.Data.Where(osoba => !osoba.Aktivni)
				.OrderBy(item => item.Prijmeni).ThenBy(item => item.Jmeno)
				.Select(item => new OsobaDto
				{
					Id = item.Id,
					PrijmeniJmeno = item.PrijmeniJmeno
				}).ToList()
			};
			return result;
		}
	}
}

