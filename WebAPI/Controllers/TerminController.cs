using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypeLite;

namespace Havit.VolejbalApp.WebAPI.Controllers
{
	public class TerminController
	{
		[HttpGet("/api/terminy")]
		public TerminResultModel Get()
		{
			return new TerminResultModel
			{
				Terminy = new TerminModel[]
				{
					new TerminModel
					{
						Id = 1,
						Datum = DateTime.Today
					},
					new TerminModel
					{
						Id = 2,
						Datum = DateTime.Today.AddDays(7)
					}
				}
			};
		}

		[HttpGet("/api/terminy/{terminId}")]
		public PrihlaskyTerminuResult Get(int terminId)
		{
			return new PrihlaskyTerminuResult
			{
				Prihlaseni = new Osoba[]
				{
					new Osoba
					{

					Id = 1,
					Name = "Kanda Jiří"
					}
				},
				Neprihlaseni = new Osoba[] { }
			};
		}

		[HttpPost("/api/terminy/{terminId}/prihlasit")]
		public void Prihlasit(int terminId, PrihlasitOdhlasitIM prihlasitData)
		{

		}

		[HttpPost("/api/terminy/{terminId}/odhlasit")]
		public void Odhlasit(int terminId, PrihlasitOdhlasitIM odhlasitData)
		{

		}

	}

	[TsClass]
	public class TerminResultModel
	{
		public TerminModel[] Terminy { get; set; }
	}

	[TsClass]
	public class TerminModel
	{
		public int Id { get; set; }
		public DateTime Datum { get; set; }
	}

	public class PrihlasitOdhlasitIM
	{
		public int OsobaId { get; set; }
	}

	public class PrihlaskyTerminuResult
	{
		public Osoba[] Prihlaseni { get; set; }
		public Osoba[] Neprihlaseni { get; set; }
	}

	public class Osoba
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

}
