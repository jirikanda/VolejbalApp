using KandaEu.Volejbal.Contracts.Reporty.Dto;
using ProtoBuf.Grpc.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Reporty
{
	public interface IReportOsobFacade
	{
		Task<ReportOsobResponse> GetReport();
	}
}
