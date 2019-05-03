using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TypeLite;
using TypeLite.Net4;
using TypeLite.TsModels;

namespace Havit.VolejbalApp.ApiModelsGenerator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
			GenerateTypescriptModels("..\\..\\..\\..\\..\\Web\\ClientApp\\apimodels", typeof(KandaEu.Volejbal.Facades.Properties.AssemblyInfo).Assembly.GetTypes());
			GenerateTypescriptModels("..\\..\\..\\..\\..\\Web\\ClientApp\\configuration", new Type[] { typeof(Web.Controllers.ViewModels.IndexViewModel.AspPrerenderData) });
			GenerateTypescriptModels("..\\..\\..\\..\\..\\Web\\ClientApp\\apimodels", new Type[] { typeof(KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions.ValidationErrorModel) }, "ValidationErrors");

			Console.WriteLine($"Completed.");
        }

		public static void GenerateTypescriptModels(string targetPath, Type[] targetTypes, string moduleName = null)
		{
			var targetModules = targetTypes.Where(type => type.GetCustomAttributes(typeof(TsClassAttribute), false).Any()).GroupBy(type => moduleName ?? type.Namespace.Substring(type.Namespace.LastIndexOf('.') + 1),
				type => type,
				(expectedModuleName, types) => new { ModuleName = expectedModuleName, Types = types.ToList() }).OrderBy(item => item.ModuleName).ToList();

			foreach (var targetModule in targetModules)
			{
				TypeScriptFluent ts = TypeScript.Definitions();
				targetModule.Types.ForEach(type => ts.For(type));
				ts = ts.WithMemberFormatter(FormatMemberName) // "cammelCase"					
					.WithModuleNameFormatter(tsModule => String.Empty) // při String.Empty se v TS negenerují namespaces (null nelze použít)
					.WithVisibility((TsClass tsClass, string typeName) => true) // generuje k typu klíčové slovo export
					.WithJSDoc() // generování JDoc; pro fungování je třeba, aby C# compiler generovar dokumentační XML
					.AsConstEnums(false);
				List<string> sharedNames = ts.ModelBuilder.Build().Classes.GroupBy(item => item.Name).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
				if (sharedNames.Count > 0)
				{
					throw new InvalidOperationException("V cílovém typescriptu došlo ke kolizi jmen těchto tříd: " + String.Join(", ", sharedNames));
				}

				string targetFilename = $"{targetPath}\\{targetModule.ModuleName}.ts";
				Console.WriteLine($"Writing {targetModule.ModuleName} module...");

				string content = "/* tslint:disable */\r\n" + ts.Generate();
				System.IO.File.WriteAllText(targetFilename, content, Encoding.UTF8);
			}
			Console.WriteLine($"Completed for path {targetPath}.");
		}

        private static string FormatMemberName(TsProperty identifier)
        {
            JsonPropertyAttribute jsonPropertyAttribute = identifier.MemberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), false).Cast<JsonPropertyAttribute>().FirstOrDefault();
            return String.IsNullOrEmpty(jsonPropertyAttribute?.PropertyName)
                ? identifier.Name[0].ToString().ToLower() + identifier.Name.Substring(1)
                : jsonPropertyAttribute.PropertyName;
        }
    }
}
