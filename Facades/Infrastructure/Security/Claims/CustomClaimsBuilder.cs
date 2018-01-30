using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using Havit.Diagnostics.Contracts;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.NewProjectTemplate.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Havit.NewProjectTemplate.Facades.Infrastructure.Security.Claims
{
    [Service(Profile = ServiceProfiles.WebAPI)]
    public class CustomClaimsBuilder : ICustomClaimsBuilder
    {
	    private readonly IUserContextInfoBuilder userContextInfoBuilder;

	    public CustomClaimsBuilder(IUserContextInfoBuilder userContextInfoBuilder)
        {
	        this.userContextInfoBuilder = userContextInfoBuilder;
        }

		/// <summary>
		/// Získá custom claims pro daný principal.
		/// </summary>
        public List<Claim> GetCustomClaims(ClaimsPrincipal principal)
        {
			Contract.Requires(principal.Identity.IsAuthenticated);

	        List<Claim> result = new List<Claim>();

	        UserContextInfo userContextInfo = userContextInfoBuilder.GetUserContextInfo(principal);
			
			// TODO: Doplnit custom claims (nebo odstranit celou mašinérii v této složce.

			return result;
        }
    }
}
