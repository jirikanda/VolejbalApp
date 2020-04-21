using Havit.Data.Patterns.UnitOfWorks;
using Havit.Extensions.DependencyInjection.Abstractions;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.DataLayer.DataSources;
using KandaEu.Volejbal.DataLayer.Repositories;
using KandaEu.Volejbal.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

		public async Task VlozOsobu(VlozOsobuRequest vlozOsobuRequest)
		{
			Osoba osoba = new Osoba
			{
				Jmeno = vlozOsobuRequest.Jmeno,
				Prijmeni = vlozOsobuRequest.Prijmeni,
				Email = vlozOsobuRequest.Email
			};

			unitOfWork.AddForInsert(osoba);
			await unitOfWork.CommitAsync();
		}

		public async Task AktivujNeaktivniOsobu(AktivujNeaktivniOsobuRequest aktivujNeaktivniOsobuRequest)
		{
			Osoba osoba = await osobaRepository.GetObjectAsync(aktivujNeaktivniOsobuRequest.OsobaId);

			CheckNeaktivniNesmazana(osoba);
			
			osoba.Aktivni = true;

			unitOfWork.AddForUpdate(osoba);
			await unitOfWork.CommitAsync();
		}

		public async Task SmazNeaktivniOsobu(SmazNeaktivniOsobuRequest request)
		{
			Osoba osoba = await osobaRepository.GetObjectAsync(request.OsobaId);

			unitOfWork.AddForDelete(osoba);
			await unitOfWork.CommitAsync();
		}

		private void CheckNeaktivniNesmazana(Osoba osoba)
		{
			osoba.ThrowIfDeleted();
			osoba.ThrowIfAktivni();
		}


		public async Task<GetOsobyResponse> GetAktivniOsoby()
		{
			return await GetOsobyByAktivni(true);
		}

		public async Task<GetOsobyResponse> GetNeaktivniOsoby()
		{
			return await GetOsobyByAktivni(false);
		}

		public async Task<GetOsobyResponse> GetOsobyByAktivni(bool aktivni)
		{
			var result = new GetOsobyResponse
			{
				Osoby = await osobaDataSource.Data.Where(osoba => osoba.Aktivni == aktivni)
				.OrderBy(item => item.Prijmeni).ThenBy(item => item.Jmeno)
				.Select(item => new OsobaDto
				{
					Id = item.Id,
					PrijmeniJmeno = item.PrijmeniJmeno
				}).ToListAsync()
			};
			return result;
		}
	}
}

