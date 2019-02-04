using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Havit.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Havit.NewProjectTemplate.WebAPI
{
	internal class ScopedLifestyleAsPerWebRequestMiddleware
	{
		private readonly RequestDelegate next;
		private readonly IWindsorContainer container;

		public ScopedLifestyleAsPerWebRequestMiddleware(RequestDelegate next, IWindsorContainer container)
		{
			this.next = next;
			this.container = container;
		}

		public async Task Invoke(HttpContext context)
		{
			using (container.BeginScope())
			{
				var factory = container.Resolve<IServiceFactory<IMyScoped>>();
				var i1 = container.Resolve<IMyScoped>(); // PerWebRequest example
				var i2 = container.Resolve<IMyScoped>(); // PerWebRequest example
				var i3 = factory.CreateService();
				Debug.Assert(Object.ReferenceEquals(i1, i2));
				Debug.Assert(Object.ReferenceEquals(i1, i3));

				await next(context);
			}
		}
	}

}