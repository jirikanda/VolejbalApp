using System.Collections.Generic;
using System.Security.Claims;

namespace Havit.VolejbalApp.Facades.Infrastructure.Security.Claims
{
    public interface ICustomClaimsBuilder
    {
        List<Claim> GetCustomClaims(ClaimsPrincipal principal);
    }
}