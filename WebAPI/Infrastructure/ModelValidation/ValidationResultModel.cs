using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ModelValidation;

public class ValidationResultModel
{
	public int StatusCode { get; }

	public ReadOnlyCollection<FieldValidationError> Errors { get; }

	public ValidationResultModel(int statusCode, ModelStateDictionary modelState)
	{
		this.StatusCode = statusCode;
		this.Errors = FieldValidationError.FromModelState(modelState);
	}

	public static object FromModelState(int statusCode, ModelStateDictionary modelState)
	{
		return new ValidationResultModel(statusCode, modelState);
	}
}
