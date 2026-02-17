using System.Net.Http.Json;
using System.Text.Json;

namespace Orchestrator.Workflows;

public class FloodResponseWorkflow
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FloodResponseWorkflow> _logger;

    public FloodResponseWorkflow(IHttpClientFactory httpClientFactory, ILogger<FloodResponseWorkflow> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(FloodAlert alert)
    {
        _logger.LogInformation("Début du workflow pour {Area} (sévérité {Severity})", alert.Area, alert.Severity);

        var resourceClient = _httpClientFactory.CreateClient("ResourceAgent");
        var volunteerClient = _httpClientFactory.CreateClient("VolunteerAgent");
        var commsClient = _httpClientFactory.CreateClient("CommunicationAgent");

        try
        {
            var resources = await resourceClient.GetFromJsonAsync<List<Resource>>($"api/resources/available?location={alert.Area}");
            var volunteers = await volunteerClient.GetFromJsonAsync<List<Volunteer>>($"api/volunteers/match?skills=flood,medical");

            var notification = new
            {
                Recipients = volunteers?.Select(v => v.Phone).ToArray() ?? Array.Empty<string>(),
                Message = $"Alerte inondation à {alert.Area} (sévérité {alert.Severity}). Ressources disponibles : {resources?.Count ?? 0}. Bénévoles trouvés : {volunteers?.Count ?? 0}."
            };
            await commsClient.PostAsJsonAsync("api/notifications", notification);

            _logger.LogInformation("Workflow terminé avec succès.");
            return "Workflow exécuté avec succès.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur dans le workflow");
            throw;
        }
    }
}

public class Resource { public int Id { get; set; } public string Type { get; set; } = string.Empty; public string Location { get; set; } = string.Empty; public int Quantity { get; set; } }
public class Volunteer { public int Id { get; set; } public string Name { get; set; } = string.Empty; public string Phone { get; set; } = string.Empty; }
public class FloodAlert { public string Area { get; set; } = string.Empty; public string Severity { get; set; } = string.Empty; public DateTime Timestamp { get; set; } }





