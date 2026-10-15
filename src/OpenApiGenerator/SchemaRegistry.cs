using System.Text;
using Microsoft.CodeAnalysis;

namespace KandaEu.Volejbal.OpenApiGenerator;

/// <summary>
/// Překlad typů na JSON schémata. Primitivní typy a kolekce se zapisují inline, třídy a enumy jako
/// pojmenovaná schémata v components (odkaz přes $ref).
/// </summary>
/// <remarks>
/// Zrcadlí chování System.Text.Json s JsonSerializerDefaults.Web, kterým Functions odpovědi serializují:
/// camelCase názvy, [JsonPropertyName] a [JsonIgnore], enum jako číslo (jako řetězec jen
/// s [JsonConverter(typeof(JsonStringEnumConverter))] na enumu). Nullable reference types projekt nemá
/// zapnuté, takže se nullability u referenčních typů netvrdí; "required" plyne z [Required] a modifikátoru
/// required, omezení z DataAnnotations ([MaxLength], [StringLength], [MinLength], [Range], [EmailAddress],
/// [Url], [RegularExpression]).
/// </remarks>
public sealed class SchemaRegistry
{
	private const string DataAnnotationsNamespace = "System.ComponentModel.DataAnnotations";
	private const string JsonSerializationNamespace = "System.Text.Json.Serialization";

	private readonly XmlDocumentation _documentation;
	private readonly CancellationToken _cancellationToken;
	private readonly Dictionary<string, JsonObject> _schemas = new Dictionary<string, JsonObject>(StringComparer.Ordinal);
	private readonly Dictionary<ITypeSymbol, string> _names = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

	public SchemaRegistry(XmlDocumentation documentation, CancellationToken cancellationToken)
	{
		_documentation = documentation;
		_cancellationToken = cancellationToken;
	}

	public JsonObject ToJson()
	{
		JsonObject result = new JsonObject();
		foreach (string name in _schemas.Keys.OrderBy(name => name, StringComparer.Ordinal))
		{
			result.Add(name, _schemas[name]);
		}
		return result;
	}

	public JsonObject GetSchema(ITypeSymbol type)
	{
		_cancellationToken.ThrowIfCancellationRequested();

		if (IsNullableValueType(type))
		{
			return MarkNullable(GetSchema(((INamedTypeSymbol)type).TypeArguments[0]));
		}

		JsonObject primitive = GetPrimitiveSchema(type);
		if (primitive != null)
		{
			return primitive;
		}

		if (type is IArrayTypeSymbol arrayType)
		{
			return (arrayType.ElementType.SpecialType == SpecialType.System_Byte)
				? new JsonObject().Add("type", "string").Add("format", "byte")
				: new JsonObject().Add("type", "array").Add("items", GetSchema(arrayType.ElementType));
		}

		if (type.TypeKind == TypeKind.Enum)
		{
			return Reference(RegisterEnum((INamedTypeSymbol)type));
		}

		ITypeSymbol dictionaryValue = GetDictionaryValueType(type);
		if (dictionaryValue != null)
		{
			return new JsonObject().Add("type", "object").Add("additionalProperties", GetSchema(dictionaryValue));
		}

		ITypeSymbol element = GetEnumerableElementType(type);
		if (element != null)
		{
			return new JsonObject().Add("type", "array").Add("items", GetSchema(element));
		}

		if (type is INamedTypeSymbol named && ((named.TypeKind == TypeKind.Class) || (named.TypeKind == TypeKind.Struct) || (named.TypeKind == TypeKind.Interface)))
		{
			return Reference(RegisterObject(named));
		}

		// Generické parametry, dynamic, ukazatele... - "cokoli".
		return new JsonObject();
	}

	#region Primitivní typy

	private static JsonObject GetPrimitiveSchema(ITypeSymbol type)
	{
		switch (type.SpecialType)
		{
			case SpecialType.System_String:
				return new JsonObject().Add("type", "string");
			case SpecialType.System_Boolean:
				return new JsonObject().Add("type", "boolean");
			case SpecialType.System_Char:
				return new JsonObject().Add("type", "string").Add("minLength", 1).Add("maxLength", 1);
			case SpecialType.System_SByte:
			case SpecialType.System_Byte:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
			case SpecialType.System_Int32:
				return new JsonObject().Add("type", "integer").Add("format", "int32");
			case SpecialType.System_UInt32:
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
				return new JsonObject().Add("type", "integer").Add("format", "int64");
			case SpecialType.System_Single:
				return new JsonObject().Add("type", "number").Add("format", "float");
			case SpecialType.System_Double:
				return new JsonObject().Add("type", "number").Add("format", "double");
			case SpecialType.System_Decimal:
				return new JsonObject().Add("type", "number").Add("format", "decimal");
			case SpecialType.System_DateTime:
				return new JsonObject().Add("type", "string").Add("format", "date-time");
			case SpecialType.System_Object:
				return new JsonObject();
		}

		switch (type.ToDisplayString())
		{
			case "System.DateTimeOffset":
				return new JsonObject().Add("type", "string").Add("format", "date-time");
			case "System.DateOnly":
				return new JsonObject().Add("type", "string").Add("format", "date");
			case "System.TimeOnly":
				return new JsonObject().Add("type", "string").Add("format", "time");
			case "System.TimeSpan":
				return new JsonObject().Add("type", "string").Add("format", "duration");
			case "System.Guid":
				return new JsonObject().Add("type", "string").Add("format", "uuid");
			case "System.Uri":
				return new JsonObject().Add("type", "string").Add("format", "uri");
		}

		return null;
	}

	public static bool IsNullableValueType(ITypeSymbol type)
	{
		return (type is INamedTypeSymbol named) && (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T);
	}

	/// <summary>Třída/struktura, která se serializuje jako objekt s vlastnostmi (ne primitiv, kolekce ani enum).</summary>
	public static bool IsObjectType(ITypeSymbol type)
	{
		return (type is INamedTypeSymbol)
			&& !IsNullableValueType(type)
			&& (GetPrimitiveSchema(type) == null)
			&& (type.TypeKind != TypeKind.Enum)
			&& (GetDictionaryValueType(type) == null)
			&& (GetEnumerableElementType(type) == null);
	}

	private static ITypeSymbol GetDictionaryValueType(ITypeSymbol type)
	{
		foreach (INamedTypeSymbol candidate in SelfAndInterfaces(type))
		{
			if (candidate.IsGenericType && (candidate.TypeArguments.Length == 2))
			{
				string definition = candidate.OriginalDefinition.ToDisplayString();
				if ((definition == "System.Collections.Generic.IDictionary<TKey, TValue>") || (definition == "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>"))
				{
					return candidate.TypeArguments[1];
				}
			}
		}
		return null;
	}

	private static ITypeSymbol GetEnumerableElementType(ITypeSymbol type)
	{
		if (type.SpecialType == SpecialType.System_String)
		{
			return null;
		}

		foreach (INamedTypeSymbol candidate in SelfAndInterfaces(type))
		{
			if (candidate.IsGenericType && (candidate.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T))
			{
				return candidate.TypeArguments[0];
			}
		}
		return null;
	}

	private static IEnumerable<INamedTypeSymbol> SelfAndInterfaces(ITypeSymbol type)
	{
		if (type is INamedTypeSymbol named)
		{
			yield return named;
		}
		foreach (INamedTypeSymbol implemented in type.AllInterfaces)
		{
			yield return implemented;
		}
	}

	#endregion

	#region Pojmenovaná schémata

	private static JsonObject Reference(string name)
	{
		return new JsonObject().Add("$ref", "#/components/schemas/" + name);
	}

	/// <summary>
	/// OpenAPI 3.0 nedovoluje vedle $ref další klíče; nullable / description / omezení se proto přidávají
	/// obalem přes allOf.
	/// </summary>
	private static JsonObject MarkNullable(JsonObject schema)
	{
		JsonObject target = schema.ContainsKey("$ref") ? new JsonObject().Add("allOf", new JsonArray().Add(schema)) : schema;
		if (!target.ContainsKey("nullable"))
		{
			target.Add("nullable", true);
		}
		return target;
	}

	private string RegisterName(INamedTypeSymbol type)
	{
		if (_names.TryGetValue(type, out string existing))
		{
			return existing;
		}

		string name = GetSchemaName(type);
		if (_schemas.ContainsKey(name))
		{
			// Stejné jméno v jiném namespace - rozliší se plným jménem.
			name = type.ToDisplayString().Replace('.', '_').Replace('<', '_').Replace('>', '_').Replace(", ", "_");
		}

		_names.Add(type, name);
		_schemas.Add(name, null);
		return name;
	}

	private static string GetSchemaName(INamedTypeSymbol type)
	{
		if (!type.IsGenericType)
		{
			return type.Name;
		}

		StringBuilder builder = new StringBuilder(type.Name).Append("Of");
		foreach (ITypeSymbol argument in type.TypeArguments)
		{
			builder.Append((argument is INamedTypeSymbol namedArgument) ? GetSchemaName(namedArgument) : argument.Name);
		}
		return builder.ToString();
	}

	private string RegisterEnum(INamedTypeSymbol enumType)
	{
		if (_names.TryGetValue(enumType, out string existing))
		{
			return existing;
		}

		string name = RegisterName(enumType);
		List<IFieldSymbol> members = enumType.GetMembers().OfType<IFieldSymbol>().Where(field => field.HasConstantValue).ToList();
		bool asString = enumType.GetAttributes().Any(attribute =>
			OpenApiDocumentBuilder.IsAttribute(attribute.AttributeClass, JsonSerializationNamespace, "JsonConverterAttribute")
			&& (attribute.ConstructorArguments.Length > 0)
			&& (attribute.ConstructorArguments[0].Value is INamedTypeSymbol converter)
			&& (converter.Name == "JsonStringEnumConverter"));

		JsonObject schema = new JsonObject();
		if (asString)
		{
			schema.Add("type", "string").Add("enum", JsonArray.Of(members.Select(member => (object)member.Name)));
		}
		else
		{
			schema
				.Add("type", "integer")
				.Add("format", "int32")
				.Add("enum", JsonArray.Of(members.Select(member => (object)Convert.ToInt64(member.ConstantValue, System.Globalization.CultureInfo.InvariantCulture))))
				.Add("x-enum-varnames", JsonArray.Of(members.Select(member => (object)member.Name)));
		}
		schema.AddIfNotNull("description", _documentation.GetSummary(enumType));

		_schemas[name] = schema;
		return name;
	}

	private string RegisterObject(INamedTypeSymbol type)
	{
		if (_names.TryGetValue(type, out string existing))
		{
			return existing;
		}

		// Jméno se registruje před vlastnostmi kvůli rekurzivním typům.
		string name = RegisterName(type);

		JsonObject properties = new JsonObject();
		JsonArray required = new JsonArray();

		foreach (IPropertySymbol property in GetSerializedProperties(type))
		{
			string propertyName = GetJsonPropertyName(property);
			JsonObject schema = GetSchema(property.Type);
			ApplyAnnotations(ref schema, property);
			properties.Add(propertyName, schema);

			if (property.IsRequired || HasAttribute(property, DataAnnotationsNamespace, "RequiredAttribute"))
			{
				required.Add(propertyName);
			}
		}

		JsonObject objectSchema = new JsonObject()
			.Add("type", "object")
			.AddIfNotNull("description", _documentation.GetSummary(type))
			.Add("properties", properties);
		if (required.Count > 0)
		{
			objectSchema.Add("required", required);
		}

		_schemas[name] = objectSchema;
		return name;
	}

	/// <summary>Veřejné instanční vlastnosti s getterem, od předka k potomkovi, bez [JsonIgnore].</summary>
	public static IEnumerable<IPropertySymbol> GetSerializedProperties(ITypeSymbol type)
	{
		List<INamedTypeSymbol> hierarchy = new List<INamedTypeSymbol>();
		for (INamedTypeSymbol current = type as INamedTypeSymbol; (current != null) && (current.SpecialType != SpecialType.System_Object) && (current.SpecialType != SpecialType.System_ValueType); current = current.BaseType)
		{
			hierarchy.Insert(0, current);
		}

		HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
		foreach (INamedTypeSymbol current in hierarchy)
		{
			foreach (IPropertySymbol property in current.GetMembers().OfType<IPropertySymbol>())
			{
				if (property.IsStatic || property.IsIndexer || (property.GetMethod == null)
					|| (property.DeclaredAccessibility != Accessibility.Public)
					|| (property.GetMethod.DeclaredAccessibility != Accessibility.Public)
					|| IsJsonIgnored(property)
					|| !seen.Add(property.Name))
				{
					continue;
				}
				yield return property;
			}
		}
	}

	private static bool IsJsonIgnored(IPropertySymbol property)
	{
		AttributeData ignore = property.GetAttributes().FirstOrDefault(attribute => OpenApiDocumentBuilder.IsAttribute(attribute.AttributeClass, JsonSerializationNamespace, "JsonIgnoreAttribute"));
		if (ignore == null)
		{
			return false;
		}

		// [JsonIgnore(Condition = ...)]: ignoruje se jen Always (= 1, výchozí bez Condition).
		KeyValuePair<string, TypedConstant> condition = ignore.NamedArguments.FirstOrDefault(argument => argument.Key == "Condition");
		return (condition.Key == null) || (condition.Value.Value is int value && (value == 1));
	}

	private static string GetJsonPropertyName(IPropertySymbol property)
	{
		AttributeData nameAttribute = property.GetAttributes().FirstOrDefault(attribute => OpenApiDocumentBuilder.IsAttribute(attribute.AttributeClass, JsonSerializationNamespace, "JsonPropertyNameAttribute"));
		if ((nameAttribute != null) && (nameAttribute.ConstructorArguments.Length > 0) && (nameAttribute.ConstructorArguments[0].Value is string explicitName))
		{
			return explicitName;
		}
		return ToCamelCase(property.Name);
	}

	/// <summary>Stejný algoritmus jako JsonNamingPolicy.CamelCase: snižuje úvodní velká písmena, poslední z nich jen když nenásleduje malé.</summary>
	private static string ToCamelCase(string name)
	{
		if (String.IsNullOrEmpty(name) || !Char.IsUpper(name[0]))
		{
			return name;
		}

		char[] characters = name.ToCharArray();
		for (int i = 0; i < characters.Length; i++)
		{
			if ((i == 1) && !Char.IsUpper(characters[i]))
			{
				break;
			}

			bool hasNext = (i + 1) < characters.Length;
			if ((i > 0) && hasNext && !Char.IsUpper(characters[i + 1]))
			{
				break;
			}

			characters[i] = Char.ToLowerInvariant(characters[i]);
		}
		return new string(characters);
	}

	private void ApplyAnnotations(ref JsonObject schema, IPropertySymbol property)
	{
		JsonObject annotations = new JsonObject();
		annotations.AddIfNotNull("description", _documentation.GetSummary(property));

		foreach (AttributeData attribute in property.GetAttributes())
		{
			if (attribute.AttributeClass?.ContainingNamespace?.ToDisplayString() != DataAnnotationsNamespace)
			{
				continue;
			}

			switch (attribute.AttributeClass.Name)
			{
				case "MaxLengthAttribute":
					AddConstant(annotations, "maxLength", GetArgument(attribute, 0));
					break;
				case "MinLengthAttribute":
					AddConstant(annotations, "minLength", GetArgument(attribute, 0));
					break;
				case "StringLengthAttribute":
					AddConstant(annotations, "maxLength", GetArgument(attribute, 0));
					AddConstant(annotations, "minLength", GetNamedArgument(attribute, "MinimumLength"));
					break;
				case "RangeAttribute":
					AddConstant(annotations, "minimum", GetArgument(attribute, 0));
					AddConstant(annotations, "maximum", GetArgument(attribute, 1));
					break;
				case "EmailAddressAttribute":
					annotations.Add("format", "email");
					break;
				case "UrlAttribute":
					annotations.Add("format", "uri");
					break;
				case "RegularExpressionAttribute":
					AddConstant(annotations, "pattern", GetArgument(attribute, 0));
					break;
			}
		}

		if (annotations.Count == 0)
		{
			return;
		}

		if (schema.ContainsKey("$ref"))
		{
			schema = new JsonObject().Add("allOf", new JsonArray().Add(schema));
		}
		foreach (KeyValuePair<string, object> annotation in annotations.Members)
		{
			if (!schema.ContainsKey(annotation.Key))
			{
				schema.Add(annotation.Key, annotation.Value);
			}
		}
	}

	private static object GetArgument(AttributeData attribute, int index)
	{
		return (attribute.ConstructorArguments.Length > index) ? attribute.ConstructorArguments[index].Value : null;
	}

	private static object GetNamedArgument(AttributeData attribute, string name)
	{
		KeyValuePair<string, TypedConstant> argument = attribute.NamedArguments.FirstOrDefault(candidate => candidate.Key == name);
		return (argument.Key == null) ? null : argument.Value.Value;
	}

	/// <summary>Konstanty z atributů mohou být int, double i string ([Range(typeof(decimal), "0", "10")]); jiné typy se vynechají.</summary>
	private static void AddConstant(JsonObject target, string key, object value)
	{
		switch (value)
		{
			case int integer:
				target.Add(key, integer);
				break;
			case long longInteger:
				target.Add(key, longInteger);
				break;
			case double number:
				target.Add(key, number);
				break;
			case string text when key == "pattern":
				target.Add(key, text);
				break;
			case string text when Double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double parsed):
				target.Add(key, parsed);
				break;
		}
	}

	private static bool HasAttribute(ISymbol symbol, string namespaceName, string attributeName)
	{
		return symbol.GetAttributes().Any(attribute => OpenApiDocumentBuilder.IsAttribute(attribute.AttributeClass, namespaceName, attributeName));
	}

	#endregion
}
