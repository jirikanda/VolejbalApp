using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace KandaEu.Volejbal.OpenApiGenerator;

/// <summary>
/// Sestaví OpenAPI 3.0.3 dokument ze symbolů kompilace: projde assembly referencující Refit, najde veřejná
/// rozhraní s Refit HTTP atributy a z jejich metod odvodí cesty, parametry, těla a odpovědi. DTO se popíší
/// jako schémata v components podle toho, jak je serializuje System.Text.Json s webovými výchozími
/// nastaveními (camelCase, enum jako číslo).
/// </summary>
public static class OpenApiDocumentBuilder
{
	private const string RefitNamespace = "Refit";
	private static readonly Regex s_routeParameterRegex = new Regex("\\{([^}:?]+)[^}]*\\}", RegexOptions.Compiled);

	/// <summary>
	/// Vrátí JSON dokument, nebo null, když kompilace žádné Refit rozhraní nevidí.
	/// </summary>
	public static string Build(Compilation compilation, GeneratorOptions options, ImmutableArray<string> documentationFiles, CancellationToken cancellationToken)
	{
		List<INamedTypeSymbol> apiInterfaces = FindApiInterfaces(compilation, cancellationToken);
		if (apiInterfaces.Count == 0)
		{
			return null;
		}

		XmlDocumentation documentation = new XmlDocumentation(documentationFiles, cancellationToken);
		SchemaRegistry schemas = new SchemaRegistry(documentation, cancellationToken);
		INamedTypeSymbol errorResponseType = String.IsNullOrEmpty(options.ErrorResponseType)
			? null
			: compilation.GetTypeByMetadataName(options.ErrorResponseType);

		JsonObject paths = new JsonObject();
		JsonArray tags = new JsonArray();
		HashSet<string> operationIds = new HashSet<string>(StringComparer.Ordinal);
		Dictionary<string, JsonObject> pathItems = new Dictionary<string, JsonObject>(StringComparer.Ordinal);

		foreach (INamedTypeSymbol apiInterface in apiInterfaces)
		{
			cancellationToken.ThrowIfCancellationRequested();

			string tag = GetTagName(apiInterface);
			tags.Add(new JsonObject()
				.Add("name", tag)
				.AddIfNotNull("description", documentation.GetSummary(apiInterface)));

			foreach (IMethodSymbol method in GetContractMethods(apiInterface))
			{
				AttributeData httpAttribute = GetHttpMethodAttribute(method);
				if (httpAttribute == null)
				{
					continue;
				}

				string httpMethod = GetHttpMethodName(httpAttribute);
				string route = NormalizeRoute(httpAttribute.ConstructorArguments.Length > 0 ? httpAttribute.ConstructorArguments[0].Value as string : null);

				if (!pathItems.TryGetValue(route, out JsonObject pathItem))
				{
					pathItem = new JsonObject();
					pathItems.Add(route, pathItem);
				}

				pathItem.Add(httpMethod.ToLowerInvariant(), BuildOperation(method, tag, route, schemas, documentation, errorResponseType, operationIds));
			}
		}

		foreach (string route in pathItems.Keys.OrderBy(route => route, StringComparer.Ordinal))
		{
			paths.Add(route, pathItems[route]);
		}

		JsonObject document = new JsonObject()
			.Add("openapi", "3.0.3")
			.Add("info", new JsonObject()
				.Add("title", String.IsNullOrEmpty(options.Title) ? compilation.AssemblyName : options.Title)
				.Add("version", options.Version))
			.Add("tags", tags)
			.Add("paths", paths)
			.Add("components", new JsonObject().Add("schemas", schemas.ToJson()));

		return JsonWriter.Serialize(document);
	}

	#region Rozhraní a metody

	private static List<INamedTypeSymbol> FindApiInterfaces(Compilation compilation, CancellationToken cancellationToken)
	{
		List<INamedTypeSymbol> result = new List<INamedTypeSymbol>();

		IEnumerable<IAssemblySymbol> candidates = new[] { compilation.Assembly }
			.Concat(compilation.SourceModule.ReferencedAssemblySymbols)
			.Where(ReferencesRefit);

		foreach (IAssemblySymbol assembly in candidates)
		{
			CollectInterfaces(assembly.GlobalNamespace, result, cancellationToken);
		}

		return result
			.OrderBy(apiInterface => apiInterface.ToDisplayString(), StringComparer.Ordinal)
			.ToList();
	}

	private static bool ReferencesRefit(IAssemblySymbol assembly)
	{
		return assembly.Modules.Any(module => module.ReferencedAssemblies.Any(reference => reference.Name == RefitNamespace));
	}

	private static void CollectInterfaces(INamespaceSymbol namespaceSymbol, List<INamedTypeSymbol> result, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		foreach (INamedTypeSymbol type in namespaceSymbol.GetTypeMembers())
		{
			if ((type.TypeKind == TypeKind.Interface)
				&& (type.DeclaredAccessibility == Accessibility.Public)
				&& GetContractMethods(type).Any(method => GetHttpMethodAttribute(method) != null))
			{
				result.Add(type);
			}
		}

		foreach (INamespaceSymbol child in namespaceSymbol.GetNamespaceMembers())
		{
			CollectInterfaces(child, result, cancellationToken);
		}
	}

	/// <summary>Metody rozhraní včetně zděděných (Refit je také obsluhuje), v pořadí deklarace.</summary>
	private static IEnumerable<IMethodSymbol> GetContractMethods(INamedTypeSymbol apiInterface)
	{
		return apiInterface.AllInterfaces.Reverse().Concat(new[] { apiInterface })
			.SelectMany(type => type.GetMembers().OfType<IMethodSymbol>())
			.Where(method => (method.MethodKind == MethodKind.Ordinary) && !method.IsStatic);
	}

	private static AttributeData GetHttpMethodAttribute(IMethodSymbol method)
	{
		return method.GetAttributes().FirstOrDefault(attribute => InheritsFrom(attribute.AttributeClass, RefitNamespace, "HttpMethodAttribute"));
	}

	private static string GetHttpMethodName(AttributeData httpAttribute)
	{
		// GetAttribute -> GET, PostAttribute -> POST, ... (Refit odvozuje HTTP metodu stejně, jen z property Method).
		string name = httpAttribute.AttributeClass.Name;
		return name.EndsWith("Attribute", StringComparison.Ordinal)
			? name.Substring(0, name.Length - "Attribute".Length).ToUpperInvariant()
			: name.ToUpperInvariant();
	}

	private static string NormalizeRoute(string route)
	{
		string result = route ?? String.Empty;
		int query = result.IndexOf('?');
		if (query >= 0)
		{
			result = result.Substring(0, query);
		}
		return result.StartsWith("/", StringComparison.Ordinal) ? result : "/" + result;
	}

	private static string GetTagName(INamedTypeSymbol apiInterface)
	{
		string name = apiInterface.Name;
		if ((name.Length > 1) && name.StartsWith("I", StringComparison.Ordinal) && Char.IsUpper(name[1]))
		{
			name = name.Substring(1);
		}
		if (name.EndsWith("Api", StringComparison.Ordinal) && (name.Length > "Api".Length))
		{
			name = name.Substring(0, name.Length - "Api".Length);
		}
		return name;
	}

	#endregion

	#region Operace

	private static JsonObject BuildOperation(
		IMethodSymbol method,
		string tag,
		string route,
		SchemaRegistry schemas,
		XmlDocumentation documentation,
		INamedTypeSymbol errorResponseType,
		HashSet<string> operationIds)
	{
		JsonObject operation = new JsonObject()
			.Add("tags", new JsonArray().Add(tag))
			.AddIfNotNull("summary", documentation.GetSummary(method))
			.AddIfNotNull("description", documentation.GetRemarks(method))
			.Add("operationId", GetOperationId(tag, method, operationIds));

		HashSet<string> routeParameters = new HashSet<string>(
			s_routeParameterRegex.Matches(route).Cast<Match>().Select(match => match.Groups[1].Value),
			StringComparer.OrdinalIgnoreCase);

		JsonArray parameters = new JsonArray();
		JsonObject requestBody = null;

		foreach (IParameterSymbol parameter in method.Parameters)
		{
			if (IsCancellationToken(parameter.Type) || HasRefitAttribute(parameter, "HeaderCollectionAttribute") || HasRefitAttribute(parameter, "PropertyAttribute") || HasRefitAttribute(parameter, "AuthorizeAttribute"))
			{
				continue;
			}

			string name = GetAlias(parameter) ?? parameter.Name;

			if (HasRefitAttribute(parameter, "BodyAttribute"))
			{
				requestBody = new JsonObject()
					.Add("required", true)
					.Add("content", new JsonObject()
						.Add("application/json", new JsonObject().Add("schema", schemas.GetSchema(parameter.Type))));
				continue;
			}

			if (routeParameters.Contains(name))
			{
				string routeName = routeParameters.First(routeParameter => String.Equals(routeParameter, name, StringComparison.OrdinalIgnoreCase));
				parameters.Add(BuildParameter(routeName, "path", true, schemas.GetSchema(parameter.Type)));
				continue;
			}

			AttributeData headerAttribute = GetRefitAttribute(parameter, "HeaderAttribute");
			if (headerAttribute != null)
			{
				string headerName = (headerAttribute.ConstructorArguments.Length > 0) ? headerAttribute.ConstructorArguments[0].Value as string : null;
				parameters.Add(BuildParameter(headerName ?? name, "header", false, schemas.GetSchema(parameter.Type)));
				continue;
			}

			// Ostatní parametry posílá Refit v query stringu; objekt s [Query] rozkládá na jeho vlastnosti.
			if (HasRefitAttribute(parameter, "QueryAttribute") && SchemaRegistry.IsObjectType(parameter.Type))
			{
				foreach (IPropertySymbol property in SchemaRegistry.GetSerializedProperties(parameter.Type))
				{
					parameters.Add(BuildParameter(GetAlias(property) ?? property.Name, "query", false, schemas.GetSchema(property.Type)));
				}
				continue;
			}

			parameters.Add(BuildParameter(name, "query", parameter.Type.IsValueType && !SchemaRegistry.IsNullableValueType(parameter.Type), schemas.GetSchema(parameter.Type)));
		}

		if (parameters.Count > 0)
		{
			operation.Add("parameters", parameters);
		}
		operation.AddIfNotNull("requestBody", requestBody);
		operation.Add("responses", BuildResponses(method.ReturnType, schemas, errorResponseType));

		return operation;
	}

	private static JsonObject BuildParameter(string name, string location, bool required, JsonObject schema)
	{
		return new JsonObject()
			.Add("name", name)
			.Add("in", location)
			.Add("required", required)
			.Add("schema", schema);
	}

	private static string GetOperationId(string tag, IMethodSymbol method, HashSet<string> operationIds)
	{
		string methodName = method.Name.EndsWith("Async", StringComparison.Ordinal)
			? method.Name.Substring(0, method.Name.Length - "Async".Length)
			: method.Name;
		string candidate = tag + "_" + methodName;
		string result = candidate;
		int suffix = 2;
		while (operationIds.Contains(result))
		{
			result = candidate + suffix;
			suffix++;
		}
		operationIds.Add(result);
		return result;
	}

	private static JsonObject BuildResponses(ITypeSymbol returnType, SchemaRegistry schemas, INamedTypeSymbol errorResponseType)
	{
		JsonObject responses = new JsonObject();
		ITypeSymbol payload = UnwrapReturnType(returnType);

		if (payload == null)
		{
			responses.Add("204", new JsonObject().Add("description", "Provedeno, odpověď nemá tělo."));
		}
		else if (IsNamed(payload, "System.Net.Http.HttpResponseMessage") || IsNamed(payload, "Refit.IApiResponse"))
		{
			responses.Add("200", new JsonObject().Add("description", "OK"));
		}
		else if (IsNamed(payload, "System.IO.Stream") || IsNamed(payload, "System.Net.Http.HttpContent"))
		{
			responses.Add("200", new JsonObject()
				.Add("description", "OK")
				.Add("content", new JsonObject().Add("application/octet-stream", new JsonObject().Add("schema", new JsonObject().Add("type", "string").Add("format", "binary")))));
		}
		else
		{
			responses.Add("200", new JsonObject()
				.Add("description", "OK")
				.Add("content", new JsonObject().Add("application/json", new JsonObject().Add("schema", schemas.GetSchema(payload)))));
		}

		if (errorResponseType != null)
		{
			JsonObject errorContent = new JsonObject().Add("application/json", new JsonObject().Add("schema", schemas.GetSchema(errorResponseType)));
			responses.Add("422", new JsonObject()
				.Add("description", "Požadavek nelze zpracovat: chyba validace vstupu (Errors) nebo stav dat, který operaci brání (Message).")
				.Add("content", errorContent));
			responses.Add("500", new JsonObject()
				.Add("description", "Neočekávaná chyba serveru.")
				.Add("content", errorContent));
		}

		return responses;
	}

	/// <summary>
	/// Task / ValueTask -> null (bez těla); Task&lt;T&gt;, ValueTask&lt;T&gt;, IObservable&lt;T&gt; -> T;
	/// ApiResponse&lt;T&gt; / IApiResponse&lt;T&gt; -> T.
	/// </summary>
	private static ITypeSymbol UnwrapReturnType(ITypeSymbol returnType)
	{
		if (!(returnType is INamedTypeSymbol named))
		{
			return returnType;
		}

		if (!named.IsGenericType)
		{
			return (IsNamed(named, "System.Threading.Tasks.Task") || IsNamed(named, "System.Threading.Tasks.ValueTask") || named.SpecialType == SpecialType.System_Void)
				? null
				: named;
		}

		string definition = named.OriginalDefinition.ToDisplayString();
		if ((definition == "System.Threading.Tasks.Task<TResult>") || (definition == "System.Threading.Tasks.ValueTask<TResult>") || (definition == "System.IObservable<T>"))
		{
			return UnwrapReturnType(named.TypeArguments[0]);
		}
		if ((definition == "Refit.ApiResponse<T>") || (definition == "Refit.IApiResponse<T>"))
		{
			return UnwrapReturnType(named.TypeArguments[0]);
		}

		return named;
	}

	#endregion

	#region Atributy a pomocné funkce

	private static bool IsCancellationToken(ITypeSymbol type)
	{
		return IsNamed(type, "System.Threading.CancellationToken");
	}

	private static bool IsNamed(ITypeSymbol type, string fullName)
	{
		return (type != null) && (type.ToDisplayString() == fullName);
	}

	private static bool HasRefitAttribute(ISymbol symbol, string attributeName)
	{
		return GetRefitAttribute(symbol, attributeName) != null;
	}

	private static AttributeData GetRefitAttribute(ISymbol symbol, string attributeName)
	{
		return symbol.GetAttributes().FirstOrDefault(attribute => IsAttribute(attribute.AttributeClass, RefitNamespace, attributeName));
	}

	private static string GetAlias(ISymbol symbol)
	{
		AttributeData alias = GetRefitAttribute(symbol, "AliasAsAttribute");
		return ((alias != null) && (alias.ConstructorArguments.Length > 0)) ? alias.ConstructorArguments[0].Value as string : null;
	}

	internal static bool IsAttribute(INamedTypeSymbol attributeClass, string namespaceName, string attributeName)
	{
		return (attributeClass != null)
			&& (attributeClass.Name == attributeName)
			&& (attributeClass.ContainingNamespace?.ToDisplayString() == namespaceName);
	}

	internal static bool InheritsFrom(INamedTypeSymbol type, string namespaceName, string baseName)
	{
		for (INamedTypeSymbol current = type; current != null; current = current.BaseType)
		{
			if (IsAttribute(current, namespaceName, baseName))
			{
				return true;
			}
		}
		return false;
	}

	#endregion
}
