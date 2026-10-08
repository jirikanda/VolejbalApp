using System.Text;
using System.Text.Json;
using Havit;
using KandaEu.Volejbal.Api.Mcp;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KandaEu.Volejbal.Tests.Api;

/// <summary>
/// MCP server v procesu: HttpContext → McpRequestHandler → handler MCP SDK → VolejbalMcpTools nad falešnými
/// fasádami. Hlídá naše dvě části - napojení SDK na trigger a převod výjimek na chybový výsledek nástroje.
/// </summary>
[TestClass]
public class McpServerTests
{
	private const string ProtocolVersion = "2025-06-18";

	[TestMethod]
	public async Task McpServer_ToolsList_VraciVsechnyNastroje()
	{
		JsonElement response = await PostAsync(new FakePrihlaskaApi(), """{"jsonrpc":"2.0","id":1,"method":"tools/list"}""");

		List<string> nastroje = response.GetProperty("result").GetProperty("tools").EnumerateArray()
			.Select(tool => tool.GetProperty("name").GetString())
			.Order()
			.ToList();

		Assert.AreSequenceEqual(new[] { "odhlasit", "prihlasit", "seznam_hracu", "seznam_terminu" }, nastroje);
	}

	[TestMethod]
	public async Task McpServer_Prihlasit_PredaArgumentyFasadeBezeZmeny()
	{
		FakePrihlaskaApi prihlaskaApi = new FakePrihlaskaApi();

		JsonElement result = await CallToolAsync(prihlaskaApi, "prihlasit", """{"datum":"2026-01-13","osobaId":"99b41d24-b3dd-4eef-a010-f78abbdd7b42"}""");

		Assert.IsFalse(IsError(result));
		// Datum musí dorazit jako "2026-01-13" - MCP extension Functions ho dřív převáděla na DateTimeOffset.
		Assert.AreEqual(("2026-01-13", "99b41d24-b3dd-4eef-a010-f78abbdd7b42"), prihlaskaApi.PosledniPrihlaseni);
	}

	[TestMethod]
	public async Task McpServer_OsetrenaVyjimka_VraciIsErrorSTextemVyjimky()
	{
		FakePrihlaskaApi prihlaskaApi = new FakePrihlaskaApi { Vyjimka = new OperationFailedException("Osoba je neaktivní.") };

		JsonElement result = await CallToolAsync(prihlaskaApi, "prihlasit", """{"datum":"2026-01-13","osobaId":"x"}""");

		Assert.IsTrue(IsError(result));
		Assert.AreEqual("Osoba je neaktivní.", GetText(result));
	}

	[TestMethod]
	public async Task McpServer_NeosetrenaVyjimka_VraciIsErrorSObecnymTextem()
	{
		FakePrihlaskaApi prihlaskaApi = new FakePrihlaskaApi { Vyjimka = new InvalidOperationException("interní detail") };

		JsonElement result = await CallToolAsync(prihlaskaApi, "odhlasit", """{"datum":"2026-01-13","osobaId":"x"}""");

		Assert.IsTrue(IsError(result));
		Assert.AreEqual("Na serveru došlo k neočekávané chybě.", GetText(result));
	}

	[TestMethod]
	[DataRow("""{"datum":"2026-01-13"}""", "Chybí povinný argument 'osobaId'.")]
	[DataRow("""{"datum":20260113,"osobaId":"x"}""", "Argument 'datum' musí být text.")]
	public async Task McpServer_NeplatneArgumenty_VraciIsErrorANevolaFasadu(string arguments, string ocekavanyText)
	{
		// Výjimka by se nahlásila jako neošetřená, kdyby se nástroj zavolal - fasáda proto hází.
		FakePrihlaskaApi prihlaskaApi = new FakePrihlaskaApi { Vyjimka = new InvalidOperationException("Fasáda se neměla volat.") };

		JsonElement result = await CallToolAsync(prihlaskaApi, "prihlasit", arguments);

		Assert.IsTrue(IsError(result));
		Assert.AreEqual(ocekavanyText, GetText(result));
	}

	[TestMethod]
	public async Task McpServer_Get_Vraci405()
	{
		IServiceProvider serviceProvider = CreateServiceProvider(new FakePrihlaskaApi());
		DefaultHttpContext httpContext = new DefaultHttpContext();
		httpContext.Request.Method = HttpMethods.Get;

		await serviceProvider.GetRequiredService<McpRequestHandler>().HandleAsync(httpContext, serviceProvider);

		Assert.AreEqual(StatusCodes.Status405MethodNotAllowed, httpContext.Response.StatusCode);
	}

	private static async Task<JsonElement> CallToolAsync(FakePrihlaskaApi prihlaskaApi, string toolName, string arguments)
	{
		JsonElement response = await PostAsync(prihlaskaApi, $$$"""{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"{{{toolName}}}","arguments":{{{arguments}}}}}""");
		return response.GetProperty("result");
	}

	private static bool IsError(JsonElement result)
	{
		return result.TryGetProperty("isError", out JsonElement isError) && isError.GetBoolean();
	}

	private static string GetText(JsonElement result)
	{
		return result.GetProperty("content")[0].GetProperty("text").GetString();
	}

	/// <summary>
	/// Pošle JSON-RPC zprávu tak, jak by ji poslal MCP klient, a vrátí JSON-RPC odpověď z SSE těla.
	/// </summary>
	private static async Task<JsonElement> PostAsync(FakePrihlaskaApi prihlaskaApi, string body)
	{
		IServiceProvider serviceProvider = CreateServiceProvider(prihlaskaApi);
		using IServiceScope scope = serviceProvider.CreateScope();

		DefaultHttpContext httpContext = new DefaultHttpContext();
		httpContext.Request.Method = HttpMethods.Post;
		httpContext.Request.ContentType = "application/json";
		httpContext.Request.Headers.Accept = "application/json, text/event-stream";
		httpContext.Request.Headers["MCP-Protocol-Version"] = ProtocolVersion;
		httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
		MemoryStream responseBody = new MemoryStream();
		httpContext.Response.Body = responseBody;

		await serviceProvider.GetRequiredService<McpRequestHandler>().HandleAsync(httpContext, scope.ServiceProvider);

		Assert.AreEqual(StatusCodes.Status200OK, httpContext.Response.StatusCode);
		string sse = Encoding.UTF8.GetString(responseBody.ToArray());
		string data = sse.Split('\n').Single(line => line.StartsWith("data: ", StringComparison.Ordinal))["data: ".Length..];
		return JsonDocument.Parse(data).RootElement.Clone();
	}

	/// <summary>
	/// Kontejner jako v Api, jen s falešnými fasádami. Generic host kvůli službám, které handler SDK čeká
	/// (IHostApplicationLifetime) a které v Api dodává host workeru.
	/// </summary>
	private static IServiceProvider CreateServiceProvider(FakePrihlaskaApi prihlaskaApi)
	{
		HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
		builder.Services.AddScoped<ITerminApi, FakeTerminApi>();
		builder.Services.AddScoped<IOsobaApi, FakeOsobaApi>();
		builder.Services.AddSingleton<IPrihlaskaApi>(prihlaskaApi);
		builder.Services.AddVolejbalMcpServer();
		return builder.Build().Services;
	}

	private sealed class FakePrihlaskaApi : IPrihlaskaApi
	{
		public Exception Vyjimka { get; set; }
		public (string TerminId, string OsobaId) PosledniPrihlaseni { get; private set; }

		public Task PrihlasitAsync(string terminId, string osobaId, CancellationToken cancellationToken = default)
		{
			if (Vyjimka != null)
			{
				throw Vyjimka;
			}
			PosledniPrihlaseni = (terminId, osobaId);
			return Task.CompletedTask;
		}

		public Task OdhlasitAsync(string terminId, string osobaId, CancellationToken cancellationToken = default)
		{
			if (Vyjimka != null)
			{
				throw Vyjimka;
			}
			return Task.CompletedTask;
		}
	}

	private sealed class FakeTerminApi : ITerminApi
	{
		public Task<TerminListDto> GetTerminyAsync(CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new TerminListDto { Terminy = new List<TerminDto>() });
		}

		public Task<TerminDetailDto> GetDetailTerminuAsync(string terminId, CancellationToken cancellationToken = default)
		{
			throw new NotSupportedException();
		}
	}

	private sealed class FakeOsobaApi : IOsobaApi
	{
		public Task<OsobaListDto> GetOsobyAsync(CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new OsobaListDto { Osoby = new List<OsobaDto>() });
		}

		public Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task VlozOsobuAsync(OsobaInputDto osobaInputDto, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task SmazOsobuAsync(string osobaId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task AktivujOsobuAsync(string osobaId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
		public Task DeaktivujOsobuAsync(string osobaId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
	}
}
