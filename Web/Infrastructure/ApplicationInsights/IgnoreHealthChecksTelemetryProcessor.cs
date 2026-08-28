using KandaEu.Volejbal.Web.Infrastructure.HealthChecks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace KandaEu.Volejbal.Web.Infrastructure.ApplicationInsights;

/// <summary>
/// Zahazuje telemetrii health probe requestů.
///
/// ACA se na health endpoint ptá nepřetržitě (readiness po 10 s, liveness po 30 s, viz deploy/main.bicep),
/// což jsou tisíce requestů denně. Bez tohoto filtru by zaplavily Application Insights a ukusovaly
/// z denního stropu ingestace (0,25 GB, viz deploy/README.md) - přitom nenesou žádnou informaci.
/// </summary>
public class IgnoreHealthChecksTelemetryProcessor(ITelemetryProcessor _next) : ITelemetryProcessor
{
	public void Process(ITelemetry item)
	{
		if (IsHealthCheckRequest(item))
		{
			return;
		}

		_next.Process(item);
	}

	private static bool IsHealthCheckRequest(ITelemetry item)
	{
		if (item is not RequestTelemetry requestTelemetry)
		{
			return false;
		}

		return (requestTelemetry.Url != null)
			&& requestTelemetry.Url.AbsolutePath.StartsWith(HealthCheckEndpoints.Path, StringComparison.OrdinalIgnoreCase);
	}
}
