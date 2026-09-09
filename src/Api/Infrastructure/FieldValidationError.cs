using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace KandaEu.Volejbal.Api.Infrastructure;

/// <summary>
/// Chyba validace jednoho pole modelu.
/// </summary>
public class FieldValidationError
{
	public string Field { get; }

	public string Message { get; }

	public FieldValidationError(string field, string message)
	{
		Field = !String.IsNullOrEmpty(field) ? field : null;
		Message = message ?? String.Empty;
	}

	/// <summary>
	/// Převede výsledky validace přes DataAnnotations na kolekci chyb jednotlivých polí.
	/// </summary>
	public static ReadOnlyCollection<FieldValidationError> FromValidationResults(IEnumerable<ValidationResult> validationResults)
	{
		return validationResults
			.SelectMany(validationResult => validationResult.MemberNames.DefaultIfEmpty(String.Empty)
				.Select(memberName => new FieldValidationError(memberName, validationResult.ErrorMessage)))
			.ToList()
			.AsReadOnly();
	}
}
