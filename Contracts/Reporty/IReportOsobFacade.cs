using KandaEu.Volejbal.Contracts.Reporty.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Reporty;

public interface IReportOsobFacade
{
    Task<ReportOsob> GetReport();
}
