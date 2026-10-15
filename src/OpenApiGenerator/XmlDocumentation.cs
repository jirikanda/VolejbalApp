using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace KandaEu.Volejbal.OpenApiGenerator;

/// <summary>
/// Čtení XML dokumentačních komentářů (summary a remarks) do prostého textu pro description.
/// </summary>
/// <remarks>
/// Symboly z vlastní kompilace mají komentáře k dispozici přímo. U typů z referencovaných projektů
/// kompilátor komentáře nenačte - referencuje ref assembly v obj, vedle které xml neleží - proto se
/// dokumentační soubory referencovaných projektů předávají jako AdditionalFiles (viz OpenApiGenerator.props)
/// a hledají se tu podle dokumentačního id symbolu (T:, M:, P:...).
/// </remarks>
public sealed class XmlDocumentation
{
	private static readonly Regex s_crefRegex = new Regex("<see\\s+cref=\"[A-Z]:([^\"]+)\"\\s*/>", RegexOptions.Compiled);
	private static readonly Regex s_paramrefRegex = new Regex("<(?:param|typeparam)ref\\s+name=\"([^\"]+)\"\\s*/>", RegexOptions.Compiled);
	private static readonly Regex s_tagRegex = new Regex("<[^>]+>", RegexOptions.Compiled);
	private static readonly Regex s_whitespaceRegex = new Regex("\\s+", RegexOptions.Compiled);

	private readonly Dictionary<string, XElement> _externalMembers = new Dictionary<string, XElement>(StringComparer.Ordinal);
	private readonly CancellationToken _cancellationToken;

	public XmlDocumentation(ImmutableArray<string> documentationFiles, CancellationToken cancellationToken)
	{
		_cancellationToken = cancellationToken;

		foreach (string content in documentationFiles)
		{
			if (String.IsNullOrWhiteSpace(content))
			{
				continue;
			}

			XDocument document;
			try
			{
				document = XDocument.Parse(content);
			}
			catch (System.Xml.XmlException)
			{
				continue;
			}

			foreach (XElement member in document.Descendants("member"))
			{
				string name = member.Attribute("name")?.Value;
				if ((name != null) && !_externalMembers.ContainsKey(name))
				{
					_externalMembers.Add(name, member);
				}
			}
		}
	}

	public string GetSummary(ISymbol symbol)
	{
		return GetElement(symbol, "summary");
	}

	public string GetRemarks(ISymbol symbol)
	{
		return GetElement(symbol, "remarks");
	}

	private string GetElement(ISymbol symbol, string elementName)
	{
		XElement member = GetMember(symbol);
		XElement element = member?.Element(elementName);
		if (element == null)
		{
			return null;
		}

		string inner = String.Concat(element.Nodes().Select(node => node.ToString()));
		inner = s_crefRegex.Replace(inner, match => GetLastSegment(match.Groups[1].Value));
		inner = s_paramrefRegex.Replace(inner, match => match.Groups[1].Value);
		inner = s_tagRegex.Replace(inner, " ");
		inner = System.Net.WebUtility.HtmlDecode(inner);
		inner = s_whitespaceRegex.Replace(inner, " ").Trim();

		return (inner.Length == 0) ? null : inner;
	}

	private XElement GetMember(ISymbol symbol)
	{
		string xml = symbol.GetDocumentationCommentXml(cancellationToken: _cancellationToken);
		if (!String.IsNullOrWhiteSpace(xml))
		{
			try
			{
				return XDocument.Parse(xml).Root;
			}
			catch (System.Xml.XmlException)
			{
				// Vadný komentář - zkusí se ještě externí dokumentace.
			}
		}

		string id = symbol.GetDocumentationCommentId();
		return ((id != null) && _externalMembers.TryGetValue(id, out XElement member)) ? member : null;
	}

	private static string GetLastSegment(string cref)
	{
		int parenthesis = cref.IndexOf('(');
		string withoutParameters = (parenthesis >= 0) ? cref.Substring(0, parenthesis) : cref;
		int dot = withoutParameters.LastIndexOf('.');
		return (dot >= 0) ? withoutParameters.Substring(dot + 1) : withoutParameters;
	}
}
