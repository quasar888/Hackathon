using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PredictionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PredictController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public PredictController(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    [HttpPost("forecast")]
    public async Task<ActionResult<ForecastResult>> Forecast([FromBody] DemandRequest request)
    {
        var endpoint = _config["AzureOpenAI:Endpoint"] + "/openai/deployments/gpt-35-turbo/completions?api-version=2023-05-15";
        var apiKey = _config["AzureOpenAI:ApiKey"];

        var prompt = $"Prédire la demande de ressources pour une alerte de sévérité '{request.AlertType}' dans la zone {request.Location}. Retourne un nombre entier.";
        var payload = new
        {
            prompt = prompt,
            max_tokens = 10,
            temperature = 0.7
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        content.Headers.Add("api-key", apiKey);

        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var demand = doc.RootElement.GetProperty("choices")[0].GetProperty("text").GetInt32();

        return Ok(new ForecastResult { ExpectedDemand = demand });
    }
}

public class DemandRequest
{
    public int ResourceId { get; set; }
    public int Quantity { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class ForecastResult
{
    public int ExpectedDemand { get; set; }
}
