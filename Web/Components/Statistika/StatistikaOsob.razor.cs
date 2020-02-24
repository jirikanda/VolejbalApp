using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ChartJs.Blazor;
using ChartJs.Blazor.Charts;
using ChartJs.Blazor.ChartJS.PieChart;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using ChartJs.Blazor.ChartJS.BarChart;
using ChartJs.Blazor.ChartJS.Common.Enums;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Wrappers;
using KandaEu.Volejbal.Web.WebApiClients;
using KandaEu.Volejbal.Web.Components.ProgressComponent;
using ChartJs.Blazor.ChartJS.Common.Handlers;

namespace KandaEu.Volejbal.Web.Components.Statistika
{
	public partial class StatistikaOsob
	{
        [Inject]
        protected IReportWebApiClient ReportWebApiClient { get; set; }

        [CascadingParameter]
        protected Progress Progress { get; set; }

        private BarConfig barConfig;
        private int reportHeight = 300;
        private bool isLoaded = false;
        
        protected override async Task OnInitializedAsync()
        {
            barConfig = new BarConfig(ChartType.HorizontalBar)
            {
                Options = new BarOptions
                {
                    Title = new OptionsTitle
                    {
                        Display = false,
                    },
                    Responsive = false,
                    Scales = new BarScales
                    {
                        XAxes = new List<CartesianAxis>
                        {
                            new LinearCartesianAxis
                            {
                                Ticks = new LinearCartesianTicks
                                {
                                    AutoSkip = false,
                                    Min = 0,
                                    StepSize = 1
                                },                                
                            }
                        }
                    },
                    Legend = new Legend
                    {
                        Display = false,
                    }
                }
            };

            var report = await Progress.ExecuteInProgressAsync(() => ReportWebApiClient.GetReportOsobAsync());

            
            barConfig.Data.Labels.AddRange(report.UcastHracu.Select(item => item.PrijmeniJmeno).ToArray());
            
            BarDataset<Int32Wrapper> barDataSet = new BarDataset<Int32Wrapper>(ChartType.HorizontalBar)
            {                
                BackgroundColor = Enumerable.Range(0, report.UcastHracu.Count).Select(i => ColorUtil.ColorHexString(0, 0, (byte)(255 - (64 + i * 29) % 128))).ToArray()
            };

            barDataSet.AddRange(report.UcastHracu.Select(item => item.PocetTerminu).ToArray().Wrap());
            barConfig.Data.Datasets.Add(barDataSet);

            reportHeight = report.UcastHracu.Count * 40 + 60; // prostě naházeno vidlemi
            isLoaded = true;
            StateHasChanged();
        }
    }
}
