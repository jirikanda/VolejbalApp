using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Filters;
using Havit.AspNetCore.Mvc.Filters.ErrorToJson;
using Havit.NewProjectTemplate.Services.Infrastructure;
using Havit.NewProjectTemplate.WebAPI.Infrastructure.ModelValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Havit.NewProjectTemplate.WebAPI.Infrastructure.ConfigurationExtensions
{
	public static class MvcConfig
	{
		public static void AddCustomizedMvc(this IServiceCollection services, IConfiguration configuration)
		{
			var mvcBuilder = services
				.AddMvc(options =>
				{					
					// TODO: Security policy
					var defaultPolicy = new AuthorizationPolicyBuilder(AuthenticationConfig.GetAuthenticationSchemes(configuration))
						.RequireAuthenticatedUser()
						.Build();
					options.Filters.Add(new AuthorizeFilter(defaultPolicy));
					options.Filters.Add(new ValidateModelAttribute { ResultSelector = ValidationErrorModel.FromModelState() });
					options.Filters.Add(new ErrorToJsonFilter(c =>
					{
						c.Map(e => e is SecurityException, e => StatusCodes.Status403Forbidden, ValidationErrorModel.FromException(StatusCodes.Status403Forbidden), markExceptionAsHandled: e => true);
						c.Map(e => e is OperationFailedException, e => StatusCodes.Status422UnprocessableEntity, ValidationErrorModel.FromException(StatusCodes.Status422UnprocessableEntity), markExceptionAsHandled: e => true);
						c.Map(e => true /* ostatní výjimky */, e => StatusCodes.Status500InternalServerError, ValidationErrorModel.FromException(StatusCodes.Status500InternalServerError), markExceptionAsHandled: e => false);
					}) { Order = 2 }); // vlastností Order nastavujeme, aby se spustilo před ErrorMonitoringFilter
					options.Filters.AddService(typeof(ErrorMonitoringFilter), 1); // vlastností Order nastavujeme, aby se spustilo _PO_ ErrorJoJsonFilter				
				})
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				.AddDataAnnotationsLocalization();
#if DEBUG
			mvcBuilder.AddJsonOptions(options => options.SerializerSettings.Formatting = Formatting.Indented);
#endif
		}
	}
}
