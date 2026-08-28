namespace KandaEu.Volejbal.Web.Infrastructure.HealthChecks;

/// <summary>
/// Cesta health check endpointu, na který se ptají probes Azure Container Apps (viz deploy/main.bicep).
///
/// Sdílená konstanta záměrně - hodnota musí souhlasit na třech místech ve <c>Startup</c>: při mapování
/// endpointu, ve výjimce z HTTPS redirectu a ve filtru telemetrie. Čtvrté místo je bicep šablona,
/// tam se hlídat nedá.
/// </summary>
public static class HealthCheckEndpoints
{
	public const string Path = "/health";
}
