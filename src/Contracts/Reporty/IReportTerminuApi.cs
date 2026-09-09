using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.Reporty.Dto;
using Refit;

namespace KandaEu.Volejbal.Contracts.Reporty;

public interface IReportTerminuApi
{
	[Get("/" + ApiRoutes.ReportyTerminy)]
	Task<ReportTerminu> GetReportAsync(CancellationToken cancellationToken = default);
}
