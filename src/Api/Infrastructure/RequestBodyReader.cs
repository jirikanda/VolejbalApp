using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KandaEu.Volejbal.Api.Infrastructure;

/// <summary>
/// Načtení a validace těla requestu.
/// </summary>
/// <remarks>
/// Nahrazuje to, co v ASP.NET Core hostingu dělal atribut [ApiController] spolu s ValidateModelAttribute:
/// ASP.NET Core integrace ve Functions nedává k dispozici MVC pipeline ani její filtry, takže se model
/// deserializuje a validuje ručně. Tvar chybové odpovědi (422 + ValidationErrorModel) zůstává stejný.
/// </remarks>
public static class RequestBodyReader
{
	/// <summary>
	/// Načte tělo requestu jako <typeparamref name="TModel"/> a zvaliduje ho přes DataAnnotations.
	/// V návratové trojici je IsValid true, pokud je model načtený a validní; jinak je v ErrorResult
	/// odpověď 422 s popisem chyb.
	/// </summary>
	public static async Task<(bool IsValid, TModel Model, IActionResult ErrorResult)> ReadValidatedAsync<TModel>(
		HttpRequest request,
		CancellationToken cancellationToken = default)
		where TModel : class
	{
		TModel model;
		try
		{
			model = await request.ReadFromJsonAsync<TModel>(cancellationToken);
		}
		catch (System.Text.Json.JsonException exception)
		{
			return (false, null, UnprocessableEntity(ValidationErrorModel.FromException(StatusCodes.Status422UnprocessableEntity, exception)));
		}

		if (model == null)
		{
			return (false, null, UnprocessableEntity(ValidationErrorModel.FromValidationResults(
				StatusCodes.Status422UnprocessableEntity,
				[new ValidationResult("Tělo requestu je prázdné.")])));
		}

		List<ValidationResult> validationResults = new List<ValidationResult>();
		if (!Validator.TryValidateObject(model, new ValidationContext(model), validationResults, validateAllProperties: true))
		{
			return (false, null, UnprocessableEntity(ValidationErrorModel.FromValidationResults(
				StatusCodes.Status422UnprocessableEntity,
				validationResults)));
		}

		return (true, model, null);
	}

	private static IActionResult UnprocessableEntity(ValidationErrorModel validationErrorModel)
	{
		return new ObjectResult(validationErrorModel) { StatusCode = StatusCodes.Status422UnprocessableEntity };
	}
}
