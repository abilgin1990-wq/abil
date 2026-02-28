using System.Text.Json.Serialization;

namespace TeklifHazirlama;

public class AppState
{
    public List<Offer> Offers { get; set; } = [];
    public SettingsData Settings { get; set; } = new();
}

public class Offer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CompanyName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public List<InstallationGroup> InstallationGroups { get; set; } = [];

    [JsonIgnore]
    public bool HasOutdatedMaterials => InstallationGroups.Any(g => g.HasOutdatedMaterials);

    [JsonIgnore]
    public string DisplayCompanyName => HasOutdatedMaterials ? $"*{CompanyName}" : CompanyName;

    [JsonIgnore]
    public decimal TotalAmount => InstallationGroups.Sum(g => g.TotalAmount);

    [JsonIgnore]
    public decimal TotalWithVat => TotalAmount * 1.20m;
}

public class InstallationGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<WorkDetail> WorkDetails { get; set; } = [];

    [JsonIgnore]
    public bool HasOutdatedMaterials => WorkDetails.Any(d => d.HasOutdatedMaterials);

    [JsonIgnore]
    public string DisplayName => HasOutdatedMaterials ? $"*{Name}" : Name;

    [JsonIgnore]
    public decimal TotalAmount => WorkDetails.Sum(d => d.GrandTotal);
}

public class WorkDetail
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<MaterialSelection> Materials { get; set; } = [];

    [JsonIgnore]
    public bool HasOutdatedMaterials => Materials.Any(m => m.IsCatalogOutdated);

    [JsonIgnore]
    public string DisplayName => HasOutdatedMaterials ? $"*{Name}" : Name;

    [JsonIgnore]
    public decimal MaterialTotal => Materials.Sum(m => m.MaterialTotal);

    [JsonIgnore]
    public decimal LaborTotal => Materials.Sum(m => m.LaborTotal);

    [JsonIgnore]
    public decimal GrandTotal => Materials.Sum(m => m.GrandTotal);
}

public class MaterialCatalogItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InstallationGroupName { get; set; } = string.Empty;
    public string WorkDetailName { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal ListPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal LaborUnitPrice { get; set; }

    [JsonIgnore]
    public decimal UnitPrice => ListPrice * (1 - DiscountPercent / 100m);
}

public class MaterialSelection
{
    public Guid MaterialCatalogItemId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ListPrice { get; set; }
    public decimal OriginalListPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal DiscountPercent { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LaborUnitPrice { get; set; }
    public bool IsCatalogOutdated { get; set; }
    public bool IsManualOverride { get; set; }

    [JsonIgnore]
    public string DisplayMaterialName => IsCatalogOutdated ? $"*{MaterialName}" : MaterialName;

    [JsonIgnore]
    public decimal MaterialTotal => Quantity * UnitPrice;

    [JsonIgnore]
    public decimal LaborTotal => Quantity * LaborUnitPrice;

    [JsonIgnore]
    public decimal GrandTotal => MaterialTotal + LaborTotal;
}

public class SettingsData
{
    public List<string> InstallationGroupTemplates { get; set; } = [];
    public Dictionary<string, List<string>> WorkDetailTemplatesByGroup { get; set; } = [];
    public List<MaterialCatalogItem> MaterialCatalog { get; set; } = [];
    public decimal DollarRate { get; set; } = 1;
    public decimal EuroRate { get; set; } = 1;
}
