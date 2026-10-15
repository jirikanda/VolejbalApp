using System.Globalization;
using System.Text;

namespace KandaEu.Volejbal.OpenApiGenerator;

/// <summary>
/// JSON objekt se zachovaným pořadím klíčů. Hodnoty: string, bool, int, long, double, JsonObject, JsonArray, null.
/// </summary>
/// <remarks>
/// Vlastní minimální model místo System.Text.Json: generátor běží uvnitř kompilátoru a každá další závislost
/// se musí distribuovat vedle analyzátoru, což u ProjectReference není spolehlivé.
/// </remarks>
public sealed class JsonObject
{
	private readonly List<KeyValuePair<string, object>> _members = new List<KeyValuePair<string, object>>();

	public int Count => _members.Count;

	public IEnumerable<KeyValuePair<string, object>> Members => _members;

	public JsonObject Add(string key, object value)
	{
		_members.Add(new KeyValuePair<string, object>(key, value));
		return this;
	}

	/// <summary>Přidá položku jen tehdy, když hodnota není null (OpenAPI nemá rádo prázdné klíče).</summary>
	public JsonObject AddIfNotNull(string key, object value)
	{
		if (value != null)
		{
			Add(key, value);
		}
		return this;
	}

	public bool ContainsKey(string key)
	{
		return _members.Any(member => member.Key == key);
	}

	public object this[string key]
	{
		get
		{
			return _members.FirstOrDefault(member => member.Key == key).Value;
		}
	}
}

public sealed class JsonArray
{
	private readonly List<object> _items = new List<object>();

	public int Count => _items.Count;

	public IEnumerable<object> Items => _items;

	public JsonArray Add(object item)
	{
		_items.Add(item);
		return this;
	}

	public static JsonArray Of(IEnumerable<object> items)
	{
		JsonArray result = new JsonArray();
		foreach (object item in items)
		{
			result.Add(item);
		}
		return result;
	}
}

public static class JsonWriter
{
	private const string Indent = "  ";

	public static string Serialize(JsonObject root)
	{
		StringBuilder builder = new StringBuilder();
		WriteValue(builder, root, 0);
		builder.Append('\n');
		return builder.ToString();
	}

	private static void WriteValue(StringBuilder builder, object value, int depth)
	{
		switch (value)
		{
			case null:
				builder.Append("null");
				break;
			case string text:
				WriteString(builder, text);
				break;
			case bool boolean:
				builder.Append(boolean ? "true" : "false");
				break;
			case int integer:
				builder.Append(integer.ToString(CultureInfo.InvariantCulture));
				break;
			case long longInteger:
				builder.Append(longInteger.ToString(CultureInfo.InvariantCulture));
				break;
			case double number:
				builder.Append(number.ToString("R", CultureInfo.InvariantCulture));
				break;
			case JsonObject jsonObject:
				WriteObject(builder, jsonObject, depth);
				break;
			case JsonArray jsonArray:
				WriteArray(builder, jsonArray, depth);
				break;
			default:
				throw new NotSupportedException($"JSON hodnota typu {value.GetType()} není podporovaná.");
		}
	}

	private static void WriteObject(StringBuilder builder, JsonObject jsonObject, int depth)
	{
		if (jsonObject.Count == 0)
		{
			builder.Append("{}");
			return;
		}

		builder.Append("{\n");
		bool first = true;
		foreach (KeyValuePair<string, object> member in jsonObject.Members)
		{
			if (!first)
			{
				builder.Append(",\n");
			}
			first = false;
			AppendIndent(builder, depth + 1);
			WriteString(builder, member.Key);
			builder.Append(": ");
			WriteValue(builder, member.Value, depth + 1);
		}
		builder.Append('\n');
		AppendIndent(builder, depth);
		builder.Append('}');
	}

	private static void WriteArray(StringBuilder builder, JsonArray jsonArray, int depth)
	{
		if (jsonArray.Count == 0)
		{
			builder.Append("[]");
			return;
		}

		builder.Append("[\n");
		bool first = true;
		foreach (object item in jsonArray.Items)
		{
			if (!first)
			{
				builder.Append(",\n");
			}
			first = false;
			AppendIndent(builder, depth + 1);
			WriteValue(builder, item, depth + 1);
		}
		builder.Append('\n');
		AppendIndent(builder, depth);
		builder.Append(']');
	}

	private static void AppendIndent(StringBuilder builder, int depth)
	{
		for (int i = 0; i < depth; i++)
		{
			builder.Append(Indent);
		}
	}

	private static void WriteString(StringBuilder builder, string text)
	{
		builder.Append('"');
		foreach (char character in text)
		{
			switch (character)
			{
				case '"':
					builder.Append("\\\"");
					break;
				case '\\':
					builder.Append("\\\\");
					break;
				case '\n':
					builder.Append("\\n");
					break;
				case '\r':
					builder.Append("\\r");
					break;
				case '\t':
					builder.Append("\\t");
					break;
				default:
					if (character < ' ')
					{
						builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
					}
					else
					{
						builder.Append(character);
					}
					break;
			}
		}
		builder.Append('"');
	}
}
