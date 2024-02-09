using KandaEu.Volejbal.WebAPI.Infrastructure.ModelValidation;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;

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
