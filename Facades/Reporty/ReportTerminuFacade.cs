using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using KandaEu.Volejbal.DataLayer.DataSources;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Havit.Extensions.DependencyInjection.Abstractions;

namespace KandaEu.Volejbal.Facades.Reporty
{
	[Service]
	public class ReportTerminuFacade : IReportTerminuFacade
	{
		private readonly ITerminDataSource terminDataSource;
		private readonly ITimeService timeService;

		public ReportTerminuFacade(ITerminDataSource terminDataSource, ITimeService timeService)
		{
			this.terminDataSource = terminDataSource;
			this.timeService = timeService;
		}

		public ReportTerminu GetReport()
		{
			DateTime today = timeService.GetCurrentDate();
			DateTime datumOdInclusive = ReportHelpers.GetZacatekSkolnihoRoku(timeService);

			return new ReportTerminu
			{
				ObsazenostTerminu = terminDataSource.Data.Where(termin => (termin.Datum >= datumOdInclusive) && (termin.Datum < today)).Select(termin => new ReportTerminuItem
				{
					Datum = termin.Datum,
					PocetHracu = termin.Prihlasky.Where(prihlaska => prihlaska.Deleted == null).Count()
				}).ToList()
			};
		}
	}
}
