using KandaEu.Volejbal.Contracts.Reporty.Dto;
using KandaEu.Volejbal.Web.Client.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Statistika;

public partial class StatistikaTerminu
{
	[Inject]
	protected IReportWebApiClient ReportWebApiClient { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	private ReportTerminu _report;

	protected override async Task OnInitializedAsync()
	{
		_report = await Progress.ExecuteInProgressAsync(() => ReportWebApiClient.GetReportTerminuAsync());
	}
}
