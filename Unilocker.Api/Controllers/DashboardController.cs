using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Unilocker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
      
        var result = new
        {
            totalSessionsToday = 42,
            activeSessions = 3,
            pendingReports = 2,
            registeredComputers = 120
        };
        return Ok(result);
    }
}
