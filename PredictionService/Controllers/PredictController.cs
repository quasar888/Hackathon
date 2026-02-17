using Microsoft.AspNetCore.Mvc;
namespace PredictionService.Controllers; [ApiController][Route("api/[controller]")]
public class PredictController : ControllerBase
{
    [HttpPost("forecast")] public ActionResult<ForecastResult> Forecast([FromBody] DemandRequest request) { var demand = request.AlertType?.ToLower() switch { "haute" => 150, "moyenne" => 80, "basse" => 30, _ => 50 }; return Ok(new ForecastResult { ExpectedDemand = demand }); }
}
public class DemandRequest { public int ResourceId { get; set; } public int Quantity { get; set; } public string AlertType { get; set; } = string.Empty; }
public class ForecastResult { public int ExpectedDemand { get; set; } }
