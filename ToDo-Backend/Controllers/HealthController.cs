using Microsoft.AspNetCore.Mvc;

namespace ToDo_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(true);
    }
}
