using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;

namespace Havit.NewProjectTemplate.WebAPI.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)] // schováme ze Swaggeru
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            if (!User.Identity.IsAuthenticated)
            {
				// TODO: Security scheme
				//throw new NotImplementedException();
	            //return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
            }

			Task.Factory.StartNew(() => throw new Exception("Exception from: Task.Factory.StartNew"));
			GC.Collect();
			return this.Redirect("swagger");
        }
    }
}
