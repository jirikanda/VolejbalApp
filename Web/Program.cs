using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Havit.NewProjectTemplate.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

	    public static IWebHost BuildWebHost(string[] args) =>
		    WebHost.CreateDefaultBuilder(args)
			    .UseApplicationInsights()
			    .UseStartup<Startup>()
#if DEBUG
			    .UseEnvironment("Development") // pro pohodlnější spuštění z command line
			    .UseUrls("http://localhost:15800") // pro pohodlnější spuštění z command line
#endif
			    .ConfigureAppConfiguration((hostContext, config) =>
			    {
				    // delete all default configuration providers
				    config.Sources.Clear();
				    config
					    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					    .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
					    .AddJsonFile(@"Config\appsettings.json", optional: true, reloadOnChange: true)
					    .AddEnvironmentVariables();
			    })
			    .ConfigureLogging((hostingContext, logging) =>
			    {
				    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
				    logging.AddConsole();
				    logging.AddDebug();
#if !DEBUG
					logging.AddEventLog();
#endif
			    })
				.Build();
    }
}
