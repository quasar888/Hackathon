namespace ResourceAgent.Models;
public class Resource { public int Id { get; set; } public string Type { get; set; } = string.Empty; public string Location { get; set; } = string.Empty; public int Quantity { get; set; } public bool IsAvailable { get; set; } }
