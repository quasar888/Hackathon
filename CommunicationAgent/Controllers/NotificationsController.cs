using Microsoft.AspNetCore.Mvc;
namespace CommunicationAgent.Controllers; [ApiController][Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    [HttpPost] public IActionResult SendNotification([FromBody] NotificationRequest request) { Console.WriteLine($"[Notification] Destinataires: {string.Join(", ", request.Recipients)}"); Console.WriteLine($"[Notification] Message: {request.Message}"); return Ok(new { status = "Notification envoyÃ©e." }); }
}
public class NotificationRequest { public string[] Recipients { get; set; } = Array.Empty<string>(); public string Message { get; set; } = string.Empty; }
