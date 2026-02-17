using Microsoft.AspNetCore.Mvc; using VolunteerAgent.Models;
namespace VolunteerAgent.Controllers; [ApiController][Route("api/[controller]")]
public class VolunteersController : ControllerBase
{
    private static readonly List<Volunteer> _volunteers = new() { new Volunteer { Id = 1, Name = "Jean", Phone = "0612345678", Skills = new List<string>{"flood","medical"}, Location = "Quartier Nord", IsAvailable = true } };
    [HttpGet("match")] public ActionResult<IEnumerable<Volunteer>> Match([FromQuery] string skills) { var skillList = skills.Split(',').Select(s => s.Trim()); var result = _volunteers.Where(v => v.IsAvailable && v.Skills.Intersect(skillList).Any()); return Ok(result); }
    [HttpPost("assign")] public IActionResult Assign([FromBody] AssignRequest request) { var v = _volunteers.FirstOrDefault(v => v.Id == request.VolunteerId && v.IsAvailable); if (v == null) return NotFound(); v.IsAvailable = false; return Ok(v); }
}
public class AssignRequest { public int VolunteerId { get; set; } public string Mission { get; set; } = string.Empty; }
