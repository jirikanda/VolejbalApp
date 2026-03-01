using KandaEu.Volejbal.Web.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Components.Pages.Statistika;

public partial class StatistikaTerminu
{
	[Inject]
	protected IReportWebApiClient ReportWebApiClient { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	private object chartOptions;
	private int chartHeight = 300;

	protected override async Task OnInitializedAsync()
	{
		var report = await Progress.ExecuteInProgressAsync(() => ReportWebApiClient.GetReportTerminuAsync());

		var labels = report.ObsazenostTerminu.Select(item => item.Datum.ToString("d.MMMM")).ToArray();
		var data = report.ObsazenostTerminu
			.Select((item, i) => new
			{
				value = item.PocetHracu,
				itemStyle = new { color = $"rgb(0,{255 - (i * 15) % 200},0)" }
			})
			.ToArray();

		chartOptions = new
		{
			grid = new { left = "90px", right = "20px", top = "10px", bottom = "10px" },
			xAxis = new { type = "value", minInterval = 1 },
			yAxis = new { type = "category", data = labels, axisLabel = new { fontSize = 11 } },
			series = new[] { new { type = "bar", data } }
		};

		chartHeight = (report.ObsazenostTerminu.Count * 30) + 40;
		StateHasChanged();
	}
}
