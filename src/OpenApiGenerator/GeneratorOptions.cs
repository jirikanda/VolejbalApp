using Microsoft.CodeAnalysis.Diagnostics;

namespace KandaEu.Volejbal.OpenApiGenerator;

/// <summary>
/// Nastavení generátoru předávané z MSBuild vlastností (CompilerVisibleProperty v OpenApiGenerator.props).
/// </summary>
/// <remarks>
/// Hodnotová rovnost je nutná: incremental generator porovnává výstupy jednotlivých kroků pipeline
/// a bez ní by se dokument přegenerovával při každé změně kompilace.
/// </remarks>
public sealed class GeneratorOptions : IEquatable<GeneratorOptions>
{
	/// <summary>Titulek v info.title. Prázdný znamená název assembly.</summary>
	public string Title { get; }

	/// <summary>Verze v info.version.</summary>
	public string Version { get; }

	/// <summary>
	/// Plné metadata jméno typu chybové odpovědi (např. KandaEu.Volejbal.Api.Infrastructure.ValidationErrorModel).
	/// Když je zadané a typ v kompilaci existuje, každá operace dostane odpovědi 422 a 500 s jeho schématem.
	/// </summary>
	public string ErrorResponseType { get; }

	/// <summary>Namespace, do kterého se vygeneruje třída s dokumentem (RootNamespace projektu).</summary>
	public string RootNamespace { get; }

	public GeneratorOptions(string title, string version, string errorResponseType, string rootNamespace)
	{
		Title = title ?? String.Empty;
		Version = String.IsNullOrWhiteSpace(version) ? "1.0" : version;
		ErrorResponseType = errorResponseType ?? String.Empty;
		RootNamespace = rootNamespace ?? String.Empty;
	}

	public static GeneratorOptions FromGlobalOptions(AnalyzerConfigOptions globalOptions)
	{
		return new GeneratorOptions(
			GetValue(globalOptions, "build_property.OpenApiTitle"),
			GetValue(globalOptions, "build_property.OpenApiVersion"),
			GetValue(globalOptions, "build_property.OpenApiErrorResponseType"),
			GetValue(globalOptions, "build_property.RootNamespace"));
	}

	private static string GetValue(AnalyzerConfigOptions globalOptions, string key)
	{
		return globalOptions.TryGetValue(key, out string value) ? value.Trim() : String.Empty;
	}

	public bool Equals(GeneratorOptions other)
	{
		return (other != null)
			&& (Title == other.Title)
			&& (Version == other.Version)
			&& (ErrorResponseType == other.ErrorResponseType)
			&& (RootNamespace == other.RootNamespace);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as GeneratorOptions);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = Title.GetHashCode();
			hash = (hash * 31) + Version.GetHashCode();
			hash = (hash * 31) + ErrorResponseType.GetHashCode();
			hash = (hash * 31) + RootNamespace.GetHashCode();
			return hash;
		}
	}
}
