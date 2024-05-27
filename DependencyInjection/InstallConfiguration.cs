using Microsoft.Extensions.Configuration;

namespace KandaEu.Volejbal.DependencyInjection;

internal class InstallConfiguration
{
	public IConfiguration Configuration { get; set; }
	public string[] ServiceProfiles { get; set; }
	public bool UseInMemoryDb { get; internal set; }
	public bool InstallOnlyLimitedHangfireExtensions { get; internal set; }
}
