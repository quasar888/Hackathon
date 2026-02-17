# fix-launcher.ps1
Write-Host "Correction du Launcher..."
$launcherProgram = ".\Launcher\Program.cs"
$newLauncherCode = @"
using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace Launcher;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Lancement de tous les services...");
        // Obtenir le chemin absolu du dossier racine de la solution (là où se trouvent les dossiers des projets)
        string solutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        Console.WriteLine($"Racine de la solution : {solutionDir}");

        var services = new[] { "ResourceAgent", "VolunteerAgent", "AlertAgent", "CommunicationAgent", "PredictionService", "Orchestrator" };
        int port = 5000;
        foreach (var svc in services)
        {
            int svcPort = svc == "Orchestrator" ? port : ++port;
            string csprojPath = Path.Combine(solutionDir, svc, $"{svc}.csproj");
            if (!File.Exists(csprojPath))
            {
                Console.WriteLine($"Fichier projet introuvable : {csprojPath}");
                continue;
            }
            Console.WriteLine($"Démarrage de {svc} sur le port {svcPort}...");
            Process.Start("dotnet", $"run --project \"{csprojPath}\" --urls=http://localhost:{svcPort}");
            await Task.Delay(2000);
        }
        Console.WriteLine("Tous les services sont démarrés. Envoi d'une alerte de test...");
        await Task.Delay(5000);
        var client = new HttpClient();
        var alert = new { area = "Quartier Nord", severity = "haute", timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") };
        try
        {
            var response = await client.PostAsJsonAsync("http://localhost:5000/api/orchestrator/flood", alert);
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Réponse de l'orchestrateur : {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'appel à l'orchestrateur : {ex.Message}");
        }
        Console.WriteLine("Appuyez sur une touche pour arrêter tous les services...");
        Console.ReadKey();
        foreach (var proc in Process.GetProcessesByName("dotnet"))
        {
            proc.Kill();
        }
    }
}
"@
Set-Content -Path $launcherProgram -Value $newLauncherCode -Encoding UTF8
Write-Host "Launcher corrigé."

Write-Host "Correction des avertissements dans l'Orchestrateur..."
$workflowFile = ".\Orchestrator\Workflows\FloodResponseWorkflow.cs"
$newWorkflow = @"
using System.Net.Http.Json;
using System.Text.Json;

namespace Orchestrator.Workflows;

public class FloodResponseWorkflow
{
    private readonly IHttpClientFactory _httpClientFactory;
    public FloodResponseWorkflow(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<string> ExecuteAsync(FloodAlert alert)
    {
        var resourceClient = _httpClientFactory.CreateClient();
        var volunteerClient = _httpClientFactory.CreateClient();
        var commsClient = _httpClientFactory.CreateClient();

        var resources = await resourceClient.GetFromJsonAsync<List<Resource>>($"http://localhost:5001/api/resources/available?location={alert.Area}");
        var volunteers = await volunteerClient.GetFromJsonAsync<List<Volunteer>>("http://localhost:5002/api/volunteers/match?skills=flood,medical");

        var notification = new
        {
            Recipients = volunteers?.Select(v => v.Phone).ToArray() ?? Array.Empty<string>(),
            Message = $"Alerte inondation à {alert.Area} (sévérité {alert.Severity}). Ressources disponibles : {resources?.Count ?? 0}. Bénévoles trouvés : {volunteers?.Count ?? 0}."
        };
        await commsClient.PostAsJsonAsync("http://localhost:5004/api/notifications", notification);

        return "Workflow exécuté avec succès.";
    }
}

public class Resource
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class Volunteer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class FloodAlert
{
    public string Area { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
"@
Set-Content -Path $workflowFile -Value $newWorkflow -Encoding UTF8
Write-Host "Workflow corrigé."

Write-Host "Recompilation de la solution..."
dotnet build ResilienceOrchestrator.sln -c Debug
Write-Host "Terminé. Vous pouvez maintenant exécuter : dotnet run --project .\Launcher"