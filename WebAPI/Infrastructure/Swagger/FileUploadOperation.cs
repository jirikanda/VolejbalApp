using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Havit.NewProjectTemplate.WebAPI.Infrastructure.Swagger
{
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                return;
            }

            // Vyhledani IFormFile názvů parametrů
            var fileParamNames = context.ApiDescription.ActionDescriptor.Parameters
                .SelectMany(x => x.ParameterType.GetProperties())
                .Where(x => x.PropertyType.IsAssignableFrom(typeof(IFormFile)))
                .Select(x => x.Name)
                .ToList();
            if (!fileParamNames.Any())
            {
                return;
            }

            var paramsToRemove = new List<IParameter>();
				
            paramsToRemove.AddRange(operation.Parameters.Where(p => fileParamNames.Contains(p.Name)));
            paramsToRemove.ForEach(x => operation.Parameters.Remove(x));

            // založení nových parametrů
            foreach (var paramName in fileParamNames)
            {
                var fileParam = new NonBodyParameter
                {
                    Type = "file",
                    Name = paramName,
                    In = "formData"
                };
                operation.Parameters.Add(fileParam);
            }
            foreach (IParameter param in operation.Parameters)
            {
                param.In = "formData";
            }

            operation.Consumes = new List<string>() { "multipart/form-data" };
        }
    }
}