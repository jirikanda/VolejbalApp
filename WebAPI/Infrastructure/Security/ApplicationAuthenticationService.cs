using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Havit.Diagnostics.Contracts;
using Havit.VolejbalApp.Facades.Infrastructure.Security.Authentication;
using Havit.VolejbalApp.Model.Security;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Havit.VolejbalApp.DataLayer.Repositories.Security;
using Havit.VolejbalApp.Facades.Infrastructure.Security.Claims;

namespace Havit.VolejbalApp.WebAPI.Infrastructure.Security
{
    /// <summary>
    /// Poskytuje uživatele z HttpContextu.
    /// </summary>
    public class ApplicationAuthenticationService : IApplicationAuthenticationService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

	    private readonly Lazy<LoginAccount> loginAccountLazy; 

		public ApplicationAuthenticationService(IHttpContextAccessor httpContextAccessor, ILoginAccountRepository loginAccountRepository)
        {
            this.httpContextAccessor = httpContextAccessor;

	        loginAccountLazy = new Lazy<LoginAccount>(() => loginAccountRepository.GetObject(GetCurrentClaimsPrincipal().GetLoginAccountId()));
		}

        public ClaimsPrincipal GetCurrentClaimsPrincipal()
        {
			return httpContextAccessor.HttpContext.User;
        }

	    public LoginAccount GetCurrentLoginAccount() => loginAccountLazy.Value;
	}
}
