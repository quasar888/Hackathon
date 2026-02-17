using System.Diagnostics;
using System.Net.Http.Json;

namespace Launcher;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Lancement de tous les services...");

        // Trouver le répertoire racine de la solution (celui qui contient le fichier .sln)
        string baseDir = AppContext.BaseDirectory;
        string solutionDir = baseDir;
        while (solutionDir != null && !Directory.GetFiles(solutionDir, "*.sln").Any())
        {
            solutionDir = Directory.GetParent(solutionDir)?.FullName;
        }
        if (solutionDir == null)
        {
            Console.WriteLine("Impossible de trouver le répertoire de la solution.");
            return;
        }
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
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{csprojPath}\" --urls=http://localhost:{svcPort}",
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal
            };
            Process.Start(psi);
            await Task.Delay(2000);
        }

        Console.WriteLine("Tous les services sont démarrés. Envoi d'une alerte de test...");
        await Task.Delay(5000);
        var client = new HttpClient();
        var alert = new { area = "Quartier Nord", severity = "haute", timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") };
        try
        {
            var response = await client.PostAsJsonAsync("http://localhost:9000/api/orchestrator/flood", alert);
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




