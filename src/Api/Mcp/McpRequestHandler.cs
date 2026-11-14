using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;

namespace KandaEu.Volejbal.Api.Mcp;

/// <summary>
/// Předává HTTP request z triggeru /mcp do Streamable HTTP handleru MCP SDK.
/// </summary>
/// <remarks>
/// <para>
/// SDK publikuje svůj handler jen přes <c>MapMcp()</c> nad <see cref="IEndpointRouteBuilder" /> - samotná třída
/// je internal. ASP.NET Core integrace isolated workeru ale endpoint routing nemá (do workeru se dostane jen
/// request spárovaný hostem s HTTP triggerem), takže zaregistrovaný endpoint by nikdy nic nedostal. Proto
/// <c>MapMcp()</c> voláme nad vlastním minimálním <see cref="IEndpointRouteBuilder" />, z vytvořených endpointů
/// si vezmeme jejich <see cref="RequestDelegate" /> a trigger ho volá přímo. Celý protokol (kontrola hlaviček
/// a verze protokolu, JSON-RPC, SSE odpověď, 202 na notifikace) tak zůstává na SDK; naše je jen tahle třída.
/// </para>
/// <para>
/// Server běží bezstavově (<c>HttpServerTransportOptions.Stateless</c>): každý POST vytvoří vlastní instanci
/// serveru, nic se nedrží v paměti mezi requesty, takže nezáleží na tom, kterou instanci Flex Consumption
/// request trefí. MapMcp v tom režimu mapuje jen POST - GET (stream zpráv od serveru) a DELETE (konec session)
/// nemají smysl a odpovídáme na ně 405, stejně jako by to udělal routing ASP.NET Core.
/// </para>
/// </remarks>
public class McpRequestHandler
{
	private readonly RequestDelegate _postRequestDelegate;

	public McpRequestHandler(IServiceProvider serviceProvider)
	{
		McpEndpointRouteBuilder endpointRouteBuilder = new McpEndpointRouteBuilder(serviceProvider);
		endpointRouteBuilder.MapMcp();

		_postRequestDelegate = endpointRouteBuilder.DataSources
			.SelectMany(dataSource => dataSource.Endpoints)
			.OfType<RouteEndpoint>()
			.Single(endpoint => endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(HttpMethods.Post) == true)
			.RequestDelegate;
	}

	/// <param name="httpContext">HttpContext requestu triggeru.</param>
	/// <param name="requestServices">DI scope invokace funkce (FunctionContext.InstanceServices) - z něj SDK
	/// v bezstavovém režimu bere služby serveru, tedy i nástroje a jejich scoped fasády.</param>
	public async Task HandleAsync(HttpContext httpContext, IServiceProvider requestServices)
	{
		if (!HttpMethods.IsPost(httpContext.Request.Method))
		{
			httpContext.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
			httpContext.Response.Headers.Allow = HttpMethods.Post;
			return;
		}

		httpContext.RequestServices = requestServices;
		await _postRequestDelegate(httpContext);
	}

	/// <summary>
	/// Jen nosič endpointů pro <c>MapMcp()</c>. Middleware pipeline nevzniká, <see cref="CreateApplicationBuilder" />
	/// MapMcp nepoužívá.
	/// </summary>
	private sealed class McpEndpointRouteBuilder(IServiceProvider _serviceProvider) : IEndpointRouteBuilder
	{
		public IServiceProvider ServiceProvider => _serviceProvider;

		public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();

		public IApplicationBuilder CreateApplicationBuilder()
		{
			return new ApplicationBuilder(_serviceProvider);
		}
	}
}
