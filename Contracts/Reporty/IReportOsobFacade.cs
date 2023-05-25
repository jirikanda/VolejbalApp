using KandaEu.Volejbal.Contracts.Reporty.Dto;

namespace KandaEu.Volejbal.Contracts.Reporty;

public interface IReportOsobFacade
{
	Task<ReportOsob> GetReport();
}
