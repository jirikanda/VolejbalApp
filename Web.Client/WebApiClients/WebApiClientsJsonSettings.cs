using System.Text.Json;

namespace KandaEu.Volejbal.Web.Client.WebApiClients;

/// <summary>
/// Server (ASP.NET Core MVC) serializuje podle JsonSerializerDefaults.Web (camelCase),
/// vygenerovaní NSwag klienti ale používají výchozí (case-sensitive, PascalCase) nastavení System.Text.Json.
/// Bez tohoto nastavení by deserializace odpovědí tiše vracela prázdné objekty.
/// </summary>
internal static class WebApiClientJsonSerializerOptions
{
	public static void Apply(JsonSerializerOptions settings)
	{
		settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
		settings.PropertyNameCaseInsensitive = true;
	}
}

public partial class NastenkaWebApiClient
{
	static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
	{
		WebApiClientJsonSerializerOptions.Apply(settings);
	}
}

public partial class OsobaWebApiClient
{
	static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
	{
		WebApiClientJsonSerializerOptions.Apply(settings);
	}
}

public partial class ReportWebApiClient
{
	static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
	{
		WebApiClientJsonSerializerOptions.Apply(settings);
	}
}

public partial class SystemWebApiClient
{
	static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
	{
		WebApiClientJsonSerializerOptions.Apply(settings);
	}
}

public partial class TerminWebApiClient
{
	static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
	{
		WebApiClientJsonSerializerOptions.Apply(settings);
	}
}
