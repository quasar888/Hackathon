using Microsoft.AspNetCore.Mvc;

namespace AlertAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private static readonly List<Alert> _alerts = new();

    [HttpPost]
    public IActionResult PostAlert([FromBody] Alert alert)
    {
        alert.Id = _alerts.Count + 1;
        alert.ReceivedAt = DateTime.UtcNow;
        _alerts.Add(alert);
        var severity = alert.Severity;
        return Ok(new { alert.Id, severity, message = "Alerte enregistrée." });
    }

    [HttpGet]
    public IActionResult GetAlerts() => Ok(_alerts);

    [HttpGet("recent")]
    public IActionResult GetRecent(int count = 5)
    {
        try
        {
            var recent = _alerts.OrderByDescending(a => a.ReceivedAt).Take(count).ToList();
            return Ok(recent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class Alert
{
    public int Id { get; set; }
    public string Area { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime ReceivedAt { get; set; }
}
