using KandaEu.Volejbal.Contracts.Reporty.Dto;
using KandaEu.Volejbal.Web.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Components.Pages.Statistika;

public partial class StatistikaTerminu
{
	[Inject]
	protected IReportWebApiClient ReportWebApiClient { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	private ReportTerminu report;

	protected override async Task OnInitializedAsync()
	{
		report = await Progress.ExecuteInProgressAsync(() => ReportWebApiClient.GetReportTerminuAsync());
	}
}
