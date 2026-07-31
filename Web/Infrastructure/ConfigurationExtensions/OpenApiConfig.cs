using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace KandaEu.Volejbal.Web.Infrastructure.ConfigurationExtensions;

public static class OpenApiConfig
{
	public static void AddCustomizedOpenApi(this IServiceCollection services)
	{
		services.AddOpenApi("current", options =>
		{
			options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0; // NSwag CLI (generátor klientů) neumí OpenAPI 3.1

			options.AddDocumentTransformer((document, context, cancellationToken) =>
			{
				document.Info.Title = "VolejbalApi";
				document.Info.Version = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(KandaEu.Volejbal.Web.Properties.AssemblyInfo).Assembly.Location).ProductVersion;
				return Task.CompletedTask;
			});

			// NSwag CLI odvozuje třídy a metody klientů z operationId ve tvaru {Controller}_{Action},
			// vestavěný generátor ale operationId pro controllery nevytváří.
			options.AddOperationTransformer((operation, context, cancellationToken) =>
			{
				if (context.Description.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
				{
					operation.OperationId = $"{controllerActionDescriptor.ControllerName}_{controllerActionDescriptor.ActionName}";
				}
				return Task.CompletedTask;
			});
		});
	}

	public static void UseCustomizedOpenApiScalarUI(this WebApplication app)
	{
		app.MapOpenApi(); // /openapi/{documentName}.json
		app.MapScalarApiReference(options => options.AddDocument("current"));
	}
}
