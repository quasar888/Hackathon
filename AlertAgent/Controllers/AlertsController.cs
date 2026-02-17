using Microsoft.AspNetCore.Mvc;
namespace AlertAgent.Controllers; [ApiController][Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private static readonly List<Alert> _alerts = new();
    [HttpPost] public IActionResult PostAlert([FromBody] Alert alert) { alert.Id = _alerts.Count + 1; alert.ReceivedAt = DateTime.UtcNow; _alerts.Add(alert); return Ok(new { alert.Id, severity = alert.Severity, message = "Alerte enregistrÃ©e." }); }
    [HttpGet] public IActionResult GetAlerts() => Ok(_alerts);
}
public class Alert { public int Id { get; set; } public string Area { get; set; } = string.Empty; public string Severity { get; set; } = string.Empty; public DateTime Timestamp { get; set; } public DateTime ReceivedAt { get; set; } }
