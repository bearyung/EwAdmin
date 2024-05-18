using Microsoft.AspNetCore.Mvc;

namespace EwAdminApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeneralController : ControllerBase
{
    // GET: api/general/health
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        // Health check endpoint for the general based API
        // return the current date and time with the status code 200 and a message
        return Ok(new
        {
            message = "Health check successful",
            dateTime = DateTime.UtcNow
        });
    }
}