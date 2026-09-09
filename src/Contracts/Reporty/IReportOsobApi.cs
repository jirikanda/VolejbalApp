using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using Refit;

namespace KandaEu.Volejbal.Contracts.Reporty;

public interface IReportOsobApi
{
	[Get("/" + ApiRoutes.ReportyOsoby)]
	Task<ReportOsob> GetReportAsync(CancellationToken cancellationToken = default);
}
