using Microsoft.AspNetCore.Mvc;

namespace Ustagram.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping()
    {
        return Ok("API is alive!");
    }
}
