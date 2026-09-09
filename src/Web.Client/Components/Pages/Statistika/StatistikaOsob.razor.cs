using KandaEu.Volejbal.Contracts.Reporty.Dto;
using KandaEu.Volejbal.Web.Client.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Statistika;

public partial class StatistikaOsob
{
	[Inject]
	protected IReportOsobApi ReportOsobApi { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	private ReportOsob _report;

	protected override async Task OnInitializedAsync()
	{
		_report = await Progress.ExecuteInProgressAsync(() => ReportOsobApi.GetReportAsync());
	}
}
