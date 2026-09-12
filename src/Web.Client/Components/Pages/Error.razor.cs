using System.Net;
using Refit;

namespace KandaEu.Volejbal.Web.Client.Components.Pages;

public partial class Error
{
	[Inject] public NavigationManager NavigationManager { get; set; }

	/// <summary>
	/// Výjimka, na které aplikace spadla. Předává ji ErrorBoundary v MainLayoutu;
	/// při přímém přechodu na /Error je null (pak zobrazíme jen obecnou hlášku).
	/// </summary>
	[Parameter] public Exception Exception { get; set; }

	private string _headline;
	private string _hint;
	private string _serverResponse;

	protected override void OnParametersSet()
	{
		(_headline, _hint) = GetDescription(Exception);
		_serverResponse = ((Exception is ApiException apiException) && apiException.HasContent) ? apiException.Content : null;
	}

	private void HandleRestartClick()
	{
		NavigationManager.NavigateTo("/", forceLoad: true);
	}

	/// <remarks>
	/// Drtivá většina pádů téhle aplikace jde z volání API, proto se vyplatí je rozlišit:
	/// ApiRequestException znamená, že požadavek k serveru vůbec nedošel (typicky neběžící API
	/// při lokálním vývoji), ApiException naopak že server odpověděl chybovým stavem.
	/// </remarks>
	private static (string Headline, string Hint) GetDescription(Exception exception)
	{
		switch (exception)
		{
			case null:
				return ("V aplikaci došlo k chybě.", null);

			case ApiRequestException apiRequestException:
				return (
					"Nepodařilo se spojit se serverem" + FormatServer(apiRequestException.Uri) + ".",
					"Server pravděpodobně neběží, není dostupný ze sítě, nebo prohlížeč zablokoval odpověď kvůli CORS. Zkuste to za chvíli znovu.");

			case ApiException apiException:
				return (
					$"Server odpověděl chybou {(int)apiException.StatusCode} ({apiException.ReasonPhrase}).",
					GetStatusCodeHint(apiException.StatusCode));

			case HttpRequestException:
				return ("Nepodařilo se spojit se serverem.", "Server neodpověděl. Zkontrolujte připojení a zkuste to za chvíli znovu.");

			case TaskCanceledException:
			case TimeoutException:
				return ("Server neodpověděl včas.", "Požadavek vypršel dřív, než přišla odpověď. Zkuste to za chvíli znovu.");

			default:
				return ("V aplikaci došlo k neočekávané chybě.", exception.Message);
		}
	}

	private static string GetStatusCodeHint(HttpStatusCode statusCode)
	{
		switch (statusCode)
		{
			case HttpStatusCode.Forbidden:
				return "Server požadovanou akci nepovolil.";

			// Fasády hlásí OperationFailedException i ObjectNotFoundException jako 422 (viz ExceptionHandlingMiddleware).
			case HttpStatusCode.UnprocessableEntity:
				return "Server požadavek odmítl - data se mezitím mohla změnit. Zkuste stránku načíst znovu.";

			case HttpStatusCode.NotFound:
				return "Server takovou adresu nezná - mohou se rozcházet verze aplikace a API.";

			default:
				return (statusCode >= HttpStatusCode.InternalServerError)
					? "Chyba nastala na straně serveru; podrobnosti jsou v jeho záznamech."
					: null;
		}
	}

	private static string FormatServer(Uri uri)
	{
		return (uri != null) && uri.IsAbsoluteUri
			? $" ({uri.Scheme}://{uri.Authority})"
			: String.Empty;
	}
}
