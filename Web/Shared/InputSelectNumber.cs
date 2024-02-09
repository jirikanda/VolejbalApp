﻿using Microsoft.AspNetCore.Components.Forms;

namespace KandaEu.Volejbal.Web.Shared;

public class InputSelectNumber<T> : InputSelect<T>
{
	protected override bool TryParseValueFromString(string value, out T result, out string validationErrorMessage)
	{
		if ((typeof(T) == typeof(int?)) && String.IsNullOrEmpty(value))
		{
			result = default;
			validationErrorMessage = null;
			return true;
		}

		if ((typeof(T) == typeof(int) || typeof(T) == typeof(int?)))
		{
			if (int.TryParse(value, out var resultInt))
			{
				result = (T)(object)resultInt;
				validationErrorMessage = null;
				return true;
			}
			else
			{
				result = default;
				validationErrorMessage = "The chosen value is not a valid number.";
				return false;
			}
		}
		else
		{
			return base.TryParseValueFromString(value, out result, out validationErrorMessage);
		}
	}
}
