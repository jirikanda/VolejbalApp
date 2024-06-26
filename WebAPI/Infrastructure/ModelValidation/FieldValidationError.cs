﻿using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ModelValidation;

public class FieldValidationError
{
	public string Field { get; }

	public string Message { get; }

	public FieldValidationError(string field, ModelError modelError)
	{
		Field = (field != string.Empty) ? field : null;
		Message = GetMessage(modelError);
	}

	private string GetMessage(ModelError modelError)
	{
		if (!String.IsNullOrEmpty(modelError.ErrorMessage))
		{
			return modelError.ErrorMessage;
		}

		if (!String.IsNullOrEmpty(modelError.Exception?.Message))
		{
			return modelError.Exception?.Message;
		}

		return String.Empty;
	}

	public static ReadOnlyCollection<FieldValidationError> FromModelState(ModelStateDictionary modelState)
	{
		return modelState.Keys
			.SelectMany(key => modelState[key].Errors.Select(modelError => new FieldValidationError(key, modelError)))
			.ToList()
			.AsReadOnly();
	}
}
