using KandaEu.Volejbal.Contracts.Reporty.Dto;
using KandaEu.Volejbal.Web.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Components.Pages.Statistika;

public partial class StatistikaOsob
{
	[Inject]
	protected IReportWebApiClient ReportWebApiClient { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	private ReportOsob report;

	protected override async Task OnInitializedAsync()
	{
		report = await Progress.ExecuteInProgressAsync(() => ReportWebApiClient.GetReportOsobAsync());
	}
}
