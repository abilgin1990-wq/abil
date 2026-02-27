using System.Text.Json;

namespace TeklifHazirlamaDesktop;

public class AppState
{
    public List<Proposal> Proposals { get; set; } = [];
    public List<PlumbingGroupCatalog> CatalogGroups { get; set; } = [];
    public List<JobDetailCatalog> CatalogDetails { get; set; } = [];
    public List<MaterialCatalogItem> Materials { get; set; } = [];
}

public class Proposal
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FirmName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
}

public class PlumbingGroupCatalog
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
}

public class JobDetailCatalog
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string GroupId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class MaterialCatalogItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string GroupId { get; set; } = string.Empty;
    public string JobDetailId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal ListPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LaborUnitPrice { get; set; }
}

public static class Storage
{
    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "teklif_hazirlama_data.json");

    public static AppState Load()
    {
        if (!File.Exists(FilePath)) return new AppState();
        var raw = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<AppState>(raw) ?? new AppState();
    }

    public static void Save(AppState state)
    {
        var raw = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, raw);
    }
}
