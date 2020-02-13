﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Filters;
using KandaEu.Volejbal.WebAPI.Infrastructure.ModelValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions
{
	public static class MvcConfig
	{
		public static void AddCustomizedMvc(this IServiceCollection services, IConfiguration configuration)
		{
			var mvcBuilder = services
				.AddControllers(options =>
				{
					//var defaultPolicy = new AuthorizationPolicyBuilder(AuthenticationConfig.GetAuthenticationSchemes(configuration))
					//	.RequireAuthenticatedUser()
					//	.Build();
					//options.Filters.Add(new AuthorizeFilter(defaultPolicy));
					options.Filters.Add(new ValidateModelAttribute { ResultSelector = ValidationErrorModel.FromModelState() });
				})
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
				.AddDataAnnotationsLocalization()
				.ConfigureApiBehaviorOptions(options =>
				{
					//options.SuppressConsumesConstraintForFormFileParameters = true;
					//options.SuppressInferBindingSourcesForParameters = true;
					options.SuppressModelStateInvalidFilter = true; // zajišťujeme pomocí ValidateModelAttribute výše
				});
#if DEBUG
			mvcBuilder.AddJsonOptions(c => c.JsonSerializerOptions.WriteIndented = true);
#endif
		}
	}
}
