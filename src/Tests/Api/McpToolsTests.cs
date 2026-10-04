using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;

namespace KandaEu.Volejbal.Tests.Api;

/// <summary>
/// Hlídá parametry MCP nástrojů (McpFunctions v projektu Api).
/// </summary>
[TestClass]
public class McpToolsTests
{
	/// <summary>
	/// MCP extension převádí argumenty, které vypadají jako datum nebo GUID, na DateTimeOffset/Guid a do
	/// string parametru je vrací přes Convert.ToString - "2026-01-13" by dorazilo jako
	/// "01/13/2026 00:00:00 +00:00". Parametry nástrojů proto musí být typované (viz McpFunctions).
	/// </summary>
	[TestMethod]
	public void McpToolProperty_ZadnyParametrNastrojeNeniString()
	{
		List<ParameterInfo> parametry = GetMcpToolProperties();

		List<string> stringParametry = parametry
			.Where(parameterInfo => parameterInfo.ParameterType == typeof(string))
			.Select(parameterInfo => $"{parameterInfo.Member.Name}.{parameterInfo.Name}")
			.ToList();

		Assert.IsEmpty(stringParametry, $"Tyto parametry MCP nástrojů jsou typu string (použij DateTimeOffset, Guid apod.):{Environment.NewLine}{String.Join(Environment.NewLine, stringParametry)}");
	}

	private static List<ParameterInfo> GetMcpToolProperties()
	{
		Assembly apiAssembly = typeof(KandaEu.Volejbal.Api.Program).Assembly;

		List<MethodInfo> mcpFunkce = apiAssembly.GetTypes()
			.SelectMany(type => type.GetMethods())
			.Where(methodInfo => methodInfo.GetCustomAttribute<FunctionAttribute>() != null)
			.Where(methodInfo => methodInfo.GetParameters().Any(parameterInfo => parameterInfo.GetCustomAttribute<McpToolTriggerAttribute>() != null))
			.ToList();

		// Pojistka: kdyby reflexe přestala MCP nástroje nacházet, test by prošel prázdný a nic by nehlídal.
		Assert.IsNotEmpty(mcpFunkce, "V assembly Api se nenašly žádné MCP nástroje - kontrola parametrů by byla bezpředmětná.");

		return mcpFunkce
			.SelectMany(methodInfo => methodInfo.GetParameters())
			.Where(parameterInfo => parameterInfo.GetCustomAttribute<McpToolPropertyAttribute>() != null)
			.ToList();
	}
}
