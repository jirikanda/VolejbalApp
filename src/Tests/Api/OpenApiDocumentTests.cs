using System.Reflection;
using System.Text.Json;
using KandaEu.Volejbal.Api.OpenApi;
using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.Terminy;
using Refit;

namespace KandaEu.Volejbal.Tests.Api;

/// <summary>
/// Hlídá OpenAPI dokument vygenerovaný při kompilaci (OpenApiGenerator): je to platný JSON, popisuje přesně
/// metody Refit kontraktu a každý $ref v něm vede na existující schéma.
/// </summary>
[TestClass]
public class OpenApiDocumentTests
{
	private static readonly Type[] s_apiInterfaces =
	[
		typeof(INastenkaApi),
		typeof(IOsobaApi),
		typeof(IPrihlaskaApi),
		typeof(ITerminApi),
		typeof(IReportOsobApi),
		typeof(IReportTerminuApi)
	];

	[TestMethod]
	public void OpenApiDocument_JePlatnyJsonSVerziOpenApi3()
	{
		using JsonDocument document = JsonDocument.Parse(GeneratedOpenApiDocument.Json);

		Assert.StartsWith("3.", document.RootElement.GetProperty("openapi").GetString());
		Assert.AreEqual("VolejbalApp API", document.RootElement.GetProperty("info").GetProperty("title").GetString());
	}

	[TestMethod]
	public void OpenApiDocument_OperaceOdpovidajiMetodamKontraktu()
	{
		using JsonDocument document = JsonDocument.Parse(GeneratedOpenApiDocument.Json);

		List<string> kontrakt = s_apiInterfaces
			.SelectMany(apiInterface => apiInterface.GetMethods().Select(GetRefitOperation))
			.OrderBy(operation => operation, StringComparer.Ordinal)
			.ToList();

		List<string> dokument = document.RootElement.GetProperty("paths").EnumerateObject()
			.SelectMany(path => path.Value.EnumerateObject().Select(operation => $"{operation.Name.ToUpperInvariant()} {path.Name}"))
			.OrderBy(operation => operation, StringComparer.Ordinal)
			.ToList();

		Assert.AreSequenceEqual(kontrakt, dokument);
	}

	[TestMethod]
	public void OpenApiDocument_MetodySTelemMajiRequestBodyAOstatniNe()
	{
		using JsonDocument document = JsonDocument.Parse(GeneratedOpenApiDocument.Json);

		foreach (Type apiInterface in s_apiInterfaces)
		{
			foreach (MethodInfo methodInfo in apiInterface.GetMethods())
			{
				bool maTelo = methodInfo.GetParameters().Any(parameter => parameter.GetCustomAttribute<BodyAttribute>() != null);
				JsonElement operation = GetOperation(document, methodInfo);

				Assert.AreEqual(maTelo, operation.TryGetProperty("requestBody", out _), $"{apiInterface.Name}.{methodInfo.Name}: requestBody");

				string ocekavanyStatus = (methodInfo.ReturnType == typeof(Task)) ? "204" : "200";
				Assert.IsTrue(operation.GetProperty("responses").TryGetProperty(ocekavanyStatus, out _), $"{apiInterface.Name}.{methodInfo.Name}: odpověď {ocekavanyStatus}");
			}
		}
	}

	[TestMethod]
	public void OpenApiDocument_KazdyRefVedeNaExistujiciSchema()
	{
		using JsonDocument document = JsonDocument.Parse(GeneratedOpenApiDocument.Json);

		HashSet<string> schemata = document.RootElement.GetProperty("components").GetProperty("schemas").EnumerateObject()
			.Select(schema => "#/components/schemas/" + schema.Name)
			.ToHashSet(StringComparer.Ordinal);

		List<string> reference = new List<string>();
		CollectReferences(document.RootElement, reference);

		Assert.IsNotEmpty(reference);
		Assert.IsEmpty(reference.Where(reference => !schemata.Contains(reference)).ToList());
	}

	private static string GetRefitOperation(MethodInfo methodInfo)
	{
		HttpMethodAttribute httpMethodAttribute = methodInfo.GetCustomAttributes().OfType<HttpMethodAttribute>().Single();
		return $"{httpMethodAttribute.Method.Method.ToUpperInvariant()} {httpMethodAttribute.Path}";
	}

	private static JsonElement GetOperation(JsonDocument document, MethodInfo methodInfo)
	{
		HttpMethodAttribute httpMethodAttribute = methodInfo.GetCustomAttributes().OfType<HttpMethodAttribute>().Single();
		return document.RootElement.GetProperty("paths").GetProperty(httpMethodAttribute.Path).GetProperty(httpMethodAttribute.Method.Method.ToLowerInvariant());
	}

	private static void CollectReferences(JsonElement element, List<string> reference)
	{
		switch (element.ValueKind)
		{
			case JsonValueKind.Object:
				foreach (JsonProperty property in element.EnumerateObject())
				{
					if ((property.Name == "$ref") && (property.Value.ValueKind == JsonValueKind.String))
					{
						reference.Add(property.Value.GetString());
					}
					else
					{
						CollectReferences(property.Value, reference);
					}
				}
				break;
			case JsonValueKind.Array:
				foreach (JsonElement item in element.EnumerateArray())
				{
					CollectReferences(item, reference);
				}
				break;
		}
	}
}
