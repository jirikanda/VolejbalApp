using KandaEu.Volejbal.Contracts.Reporty.Dto;

namespace KandaEu.Volejbal.Contracts.Reporty;

public interface IReportTerminuFacade
{
    Task<ReportTerminu> GetReport();
}
