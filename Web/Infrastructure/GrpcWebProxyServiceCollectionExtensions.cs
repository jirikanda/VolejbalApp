using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Infrastructure
{
	public static class GrpcWebProxyServiceCollectionExtensions
	{
		public static void AddGrpcWebProxy<TService>(this IServiceCollection services, IConfiguration configuration)
			where TService : class
		{
			string webAPIConnectionString = configuration.GetConnectionString("WebAPI");

			services.AddSingleton<TService>(sp =>
			{ 
				var handler = new Grpc.Net.Client.Web.GrpcWebHandler(Grpc.Net.Client.Web.GrpcWebMode.GrpcWebText, new HttpClientHandler());
				var channel = Grpc.Net.Client.GrpcChannel.ForAddress(webAPIConnectionString, new Grpc.Net.Client.GrpcChannelOptions() { HttpClient = new HttpClient(handler) });
				return channel.CreateGrpcService<TService>();
			});
		}
	}
}
