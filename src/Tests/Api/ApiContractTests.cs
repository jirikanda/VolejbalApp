using System.Reflection;
using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.System;
using KandaEu.Volejbal.Contracts.Terminy;
using Microsoft.Azure.Functions.Worker;
using Refit;

namespace KandaEu.Volejbal.Tests.Api;

/// <summary>
/// Hlídá, že se Refit kontrakt (I*Api v Contracts) nerozejde s HTTP triggery v projektu Api.
/// </summary>
/// <remarks>
/// Signatury (parametry a návratové typy) vynutí kompilátor tím, že rozhraní fasád z I*Api dědí.
/// Nevynucená zůstává vazba mezi Refit atributem a atributem HttpTrigger - tedy cesta a HTTP metoda.
/// Přesně to kontroluje tento test.
/// </remarks>
[TestClass]
public class ApiContractTests
{
	private static readonly Type[] s_apiInterfaces =
	[
		typeof(INastenkaApi),
		typeof(IOsobaApi),
		typeof(IPrihlaskaApi),
		typeof(ITerminApi),
		typeof(IReportOsobApi),
		typeof(IReportTerminuApi),
		typeof(IDataSeedApi)
	];

	[TestMethod]
	public void ApiContract_KazdaMetodaKontraktuMaOdpovidajiciHttpTrigger()
	{
		List<(string Method, string Route)> triggers = GetHttpTriggers();
		List<string> chybejici = new List<string>();

		foreach (Type apiInterface in s_apiInterfaces)
		{
			foreach (MethodInfo methodInfo in apiInterface.GetMethods())
			{
				(string httpMethod, string route) = GetRefitRoute(methodInfo);

				if (!triggers.Any(trigger => String.Equals(trigger.Route, route, StringComparison.OrdinalIgnoreCase)
					&& String.Equals(trigger.Method, httpMethod, StringComparison.OrdinalIgnoreCase)))
				{
					chybejici.Add($"{apiInterface.Name}.{methodInfo.Name}: {httpMethod} {route}");
				}
			}
		}

		Assert.IsEmpty(chybejici, $"Pro tyto metody kontraktu neexistuje HTTP trigger:{Environment.NewLine}{String.Join(Environment.NewLine, chybejici)}");
	}

	[TestMethod]
	public void ApiContract_KazdyHttpTriggerMaOdpovidajiciMetoduKontraktu()
	{
		// Health endpoint záměrně není součástí klientského kontraktu - slouží monitoringu.
		const string healthRoute = "api/health";

		List<(string HttpMethod, string Route)> kontrakt = s_apiInterfaces
			.SelectMany(apiInterface => apiInterface.GetMethods().Select(GetRefitRoute))
			.ToList();

		List<string> navic = GetHttpTriggers()
			.Where(trigger => !String.Equals(trigger.Route, healthRoute, StringComparison.OrdinalIgnoreCase))
			.Where(trigger => !kontrakt.Any(k => String.Equals(k.Route, trigger.Route, StringComparison.OrdinalIgnoreCase)
				&& String.Equals(k.HttpMethod, trigger.Method, StringComparison.OrdinalIgnoreCase)))
			.Select(trigger => $"{trigger.Method} {trigger.Route}")
			.ToList();

		Assert.IsEmpty(navic, $"Tyto HTTP triggery nejsou v klientském kontraktu:{Environment.NewLine}{String.Join(Environment.NewLine, navic)}");
	}

	private static (string HttpMethod, string Route) GetRefitRoute(MethodInfo methodInfo)
	{
		HttpMethodAttribute httpMethodAttribute = methodInfo.GetCustomAttributes()
			.OfType<HttpMethodAttribute>()
			.SingleOrDefault();

		Assert.IsNotNull(httpMethodAttribute, $"Metoda {methodInfo.DeclaringType?.Name}.{methodInfo.Name} nemá Refit HTTP atribut.");

		// Refit šablony začínají lomítkem, HttpTrigger.Route je bez něj.
		return (httpMethodAttribute.Method.Method, httpMethodAttribute.Path.TrimStart('/'));
	}

	private static List<(string Method, string Route)> GetHttpTriggers()
	{
		List<(string Method, string Route)> triggers = GetHttpTriggersCore();

		// Pojistka: kdyby reflexe přestala triggery nacházet (přejmenovaný atribut, jiný tvar funkcí),
		// oba testy výše by prošly prázdné a tiše přestaly cokoli hlídat.
		Assert.IsNotEmpty(triggers, "V assembly Api se nenašly žádné HTTP triggery - kontrola kontraktu by byla bezpředmětná.");

		return triggers;
	}

	private static List<(string Method, string Route)> GetHttpTriggersCore()
	{
		Assembly apiAssembly = typeof(KandaEu.Volejbal.Api.Program).Assembly;

		return apiAssembly.GetTypes()
			.SelectMany(type => type.GetMethods())
			.Where(methodInfo => methodInfo.GetCustomAttribute<FunctionAttribute>() != null)
			.SelectMany(methodInfo => methodInfo.GetParameters())
			.Select(parameterInfo => parameterInfo.GetCustomAttribute<HttpTriggerAttribute>())
			.Where(httpTriggerAttribute => httpTriggerAttribute != null)
			.SelectMany(httpTriggerAttribute => httpTriggerAttribute.Methods
				.Select(method => (Method: method, Route: httpTriggerAttribute.Route)))
			.ToList();
	}
}
