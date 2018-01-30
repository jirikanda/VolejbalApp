using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;

namespace Havit.NewProjectTemplate.WebAPI.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)] // schováme ze Swaggeru
    public class HomeController : Controller
    {
        [Route("")]
        [HttpGet]
        public IActionResult Get()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
            }
            return this.Redirect("swagger");
        }
    }
}
