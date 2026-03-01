using KandaEu.Volejbal.Web.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Components.Pages.Statistika;

public partial class StatistikaOsob
{
	[Inject]
	protected IReportWebApiClient ReportWebApiClient { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	private object chartOptions;
	private int chartHeight = 300;

	protected override async Task OnInitializedAsync()
	{
		var report = await Progress.ExecuteInProgressAsync(() => ReportWebApiClient.GetReportOsobAsync());

		var labels = report.UcastHracu.Select(item => item.PrijmeniJmeno).ToArray();
		var data = report.UcastHracu
			.Select((item, i) => new
			{
				value = item.PocetTerminu,
				itemStyle = new { color = $"rgb(0,0,{255 - (i * 15) % 200})" }
			})
			.ToArray();

		chartOptions = new
		{
			grid = new { left = "110px", right = "20px", top = "10px", bottom = "10px" },
			xAxis = new { type = "value", minInterval = 1 },
			yAxis = new { type = "category", data = labels, axisLabel = new { fontSize = 11 } },
			series = new[] { new { type = "bar", data } }
		};

		chartHeight = (report.UcastHracu.Count * 30) + 40;
		StateHasChanged();
	}
}
