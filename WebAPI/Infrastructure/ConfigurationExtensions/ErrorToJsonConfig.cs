using System.Security;
using KandaEu.Volejbal.Services.Infrastructure;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;

public static class ErrorToJsonConfig
{
	public static void AddCustomizedErrorToJson(this IServiceCollection services)
	{
		services.AddErrorToJson(c =>
		{
			c.Map(e => e is SecurityException, e => StatusCodes.Status403Forbidden, ValidationErrorModel.FromException(StatusCodes.Status403Forbidden), markExceptionAsHandled: e => true);
			c.Map(e => e is OperationFailedException, e => StatusCodes.Status422UnprocessableEntity, ValidationErrorModel.FromException(StatusCodes.Status422UnprocessableEntity), markExceptionAsHandled: e => true);
			c.Map(e => true /* ostatní výjimky */, e => StatusCodes.Status500InternalServerError, ValidationErrorModel.FromException(StatusCodes.Status500InternalServerError), markExceptionAsHandled: e => false);
		});
	}
}
