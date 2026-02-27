namespace TeklifHazirlama.Models;

public sealed class MaterialCatalogItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string PlumbingGroup { get; set; } = string.Empty;
    public string JobDetail { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal ListPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal UnitMaterialPrice => ListPrice * (1 - DiscountPercent / 100m);
    public string DisplayText => $"{MaterialName} - {Brand} (Liste: {ListPrice:n2} ₺, İskonto: %{DiscountPercent:n2})";
}

public sealed class PlumbingGroupQuote
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<JobDetailQuote> JobDetails { get; } = new();
    public decimal TotalAmount => JobDetails.Sum(x => x.TotalAmount);
}

public sealed class JobDetailQuote
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<QuoteMaterialLine> MaterialLines { get; } = new();
    public decimal MaterialTotal => MaterialLines.Sum(x => x.MaterialTotal);
    public decimal LaborTotal => MaterialLines.Sum(x => x.LaborTotal);
    public decimal TotalAmount => MaterialLines.Sum(x => x.LineTotal);
}

public sealed class QuoteMaterialLine
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public MaterialCatalogItem CatalogItem { get; init; } = default!;
    public decimal Quantity { get; set; }
    public decimal LaborUnitPrice { get; set; }
    public decimal UnitMaterialPrice => CatalogItem.UnitMaterialPrice;
    public decimal MaterialTotal => Quantity * UnitMaterialPrice;
    public decimal LaborTotal => Quantity * LaborUnitPrice;
    public decimal LineTotal => MaterialTotal + LaborTotal;
}
