using Microsoft.AspNetCore.Mvc;
using ResourceAgent.Models;
using System.Net.Http.Json;
using Polly;
using Polly.CircuitBreaker;
using System.Collections.Concurrent;

namespace ResourceAgent.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private static readonly List<Resource> _resources = new()
    {
        new Resource { Id = 1, Type = "Water", Location = "Quartier Nord", Quantity = 100, IsAvailable = true },
        new Resource { Id = 2, Type = "Food", Location = "Quartier Nord", Quantity = 50, IsAvailable = true },
        new Resource { Id = 3, Type = "Blankets", Location = "Quartier Sud", Quantity = 30, IsAvailable = true }
    };

    private static readonly ConcurrentDictionary<string, object> _idempotentResults = new();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ResourcesController> _logger;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;

    public ResourcesController(IHttpClientFactory httpClientFactory, ILogger<ResourcesController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _circuitBreaker = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30),
                onBreak: (ex, time) => _logger.LogWarning("Circuit ouvert pour {time} secondes", time.TotalSeconds),
                onReset: () => _logger.LogInformation("Circuit rétabli"));
    }

    [HttpGet("available")]
    public ActionResult<IEnumerable<Resource>> GetAvailable([FromQuery] string? location)
    {
        var query = _resources.Where(r => r.IsAvailable);
        if (!string.IsNullOrEmpty(location))
            query = query.Where(r => r.Location.Contains(location, StringComparison.OrdinalIgnoreCase));
        return Ok(query.ToList());
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve([FromBody] ReserveRequest request)
    {
        if (Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
        {
            var key = idempotencyKey.ToString();
            if (_idempotentResults.TryGetValue(key, out var existingResult))
                return Ok(existingResult);
        }

        var resource = _resources.FirstOrDefault(r => r.Id == request.ResourceId && r.IsAvailable);
        if (resource == null)
            return NotFound("Ressource non disponible ou introuvable.");

        var predictionClient = _httpClientFactory.CreateClient("PredictionService");

        try
        {
            var predictionResponse = await _circuitBreaker.ExecuteAsync(async () =>
                await predictionClient.PostAsJsonAsync("http://localhost:9005/api/predict/forecast", request));

            predictionResponse.EnsureSuccessStatusCode();
            var forecast = await predictionResponse.Content.ReadFromJsonAsync<ForecastResult>();

            if (forecast?.ExpectedDemand > 100)
                return BadRequest("La demande prédite est trop élevée pour cette ressource.");

            resource.Quantity -= request.Quantity;
            if (resource.Quantity <= 0)
                resource.IsAvailable = false;

            var result = new { resource.Id, remaining = resource.Quantity };

            if (Request.Headers.TryGetValue("Idempotency-Key", out var key))
                _idempotentResults.TryAdd(key.ToString(), result);

            return Ok(result);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker ouvert - service PredictionService injoignable");
            return StatusCode(503, "Service de prédiction temporairement indisponible (circuit ouvert).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel au service de prédiction");
            return StatusCode(500, "Erreur interne, veuillez réessayer plus tard.");
        }
    }
}

public class ReserveRequest { public int ResourceId { get; set; } public int Quantity { get; set; } public string AlertType { get; set; } = string.Empty; }
public class ForecastResult { public int ExpectedDemand { get; set; } }




