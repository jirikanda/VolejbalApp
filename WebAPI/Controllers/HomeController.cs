using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KandaEu.Volejbal.WebAPI.Controllers;

[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)] // schováme ze Swaggeru
public class HomeController : ControllerBase
{
    [HttpGet("")]
    public IActionResult Get()
    {
        return this.Redirect("swagger");
    }
}
