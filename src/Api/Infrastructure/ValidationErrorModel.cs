using System.ComponentModel.DataAnnotations;

namespace KandaEu.Volejbal.Api.Infrastructure;

/// <summary>
/// Třída popisuje chybu WebAPI. Tvar odpovědi je zachovaný z původního ASP.NET Core hostingu
/// (Havit.AspNetCore.Mvc ErrorToJson + ValidateModelAttribute), aby se klientovi nezměnil kontrakt.
/// </summary>
public class ValidationErrorModel
{
	/// <summary>
	/// Status code.
	/// </summary>
	public int StatusCode { get; private set; }

	/// <summary>
	/// Text chyby. Použito pro chyby vyhozené "ručně" výjimkou OperationFailedException (ev. jiné).
	/// Null hodnota není do JSON serializována.
	/// Je vzájemně výlučné s Errors. Buď je jedno, nebo druhé.
	/// </summary>
	public string Message { get; private set; }

#if DEBUG
	/// <summary>
	/// Stack trace výjimky. Jen pro aplikaci kompilovanou v DEBUGu!
	/// </summary>
	public string StackTrace { get; private set; }
#endif

	/// <summary>
	/// Chyby validace modelu.
	/// Null hodnota není do JSON serializována.
	/// Je vzájemně výlučné s Message. Buď je jedno, nebo druhé.
	/// </summary>
	public IReadOnlyCollection<FieldValidationError> Errors { get; private set; }

	private ValidationErrorModel(int statusCode)
	{
		this.StatusCode = statusCode;
	}

	/// <summary>
	/// Vrací ValidationErrorModel pro výjimku.
	/// </summary>
	public static ValidationErrorModel FromException(int statusCode, Exception exception)
	{
		return new ValidationErrorModel(statusCode)
		{
			Message = exception.Message,
#if DEBUG
			StackTrace = exception.StackTrace
#endif
		};
	}

	/// <summary>
	/// Vrací ValidationErrorModel pro výsledky validace modelu.
	/// </summary>
	public static ValidationErrorModel FromValidationResults(int statusCode, IEnumerable<ValidationResult> validationResults)
	{
		return new ValidationErrorModel(statusCode)
		{
			Errors = FieldValidationError.FromValidationResults(validationResults)
		};
	}
}
