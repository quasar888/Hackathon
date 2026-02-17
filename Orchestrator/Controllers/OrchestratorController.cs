using Microsoft.AspNetCore.Mvc; using Orchestrator.Workflows;
namespace Orchestrator.Controllers; [ApiController][Route("api/[controller]")]
public class OrchestratorController : ControllerBase
{
    private readonly FloodResponseWorkflow _workflow;
    public OrchestratorController(FloodResponseWorkflow workflow) => _workflow = workflow;
    [HttpPost("flood")] public async Task<IActionResult> TriggerFlood([FromBody] FloodAlert alert) { try { var result = await _workflow.ExecuteAsync(alert); return Ok(new { message = result }); } catch (Exception ex) { return StatusCode(500, $"Erreur : {ex.Message}"); } }
}
