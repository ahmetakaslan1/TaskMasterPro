using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TaskMasterPro.API.Models;

namespace TaskMasterPro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var report = await _healthCheckService.CheckHealthAsync();
        
        var result = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration,
            Info = report.Entries.Select(e => new { Key = e.Key, Status = e.Value.Status.ToString(), Description = e.Value.Description })
        };

        return report.Status == HealthStatus.Healthy 
            ? Ok(ApiResponse<object>.Ok(result, "System is healthy."))
            : StatusCode(503, ApiResponse<object>.Fail("System is unhealthy.", new[] { result.Status }));
    }
}
