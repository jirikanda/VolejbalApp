using KandaEu.Volejbal.Contracts.Reporty.Dto;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Reporty
{
	[ServiceContract]
	public interface IReportTerminuFacade
	{
		Task<ReportTerminu> GetReport();
	}
}
