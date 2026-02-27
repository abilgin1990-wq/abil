using TeklifHazirlama.Models;

namespace TeklifHazirlama.Services;

public sealed class InMemoryDataStore
{
    public static InMemoryDataStore Instance { get; } = new();

    public List<string> PlumbingGroups { get; } = new();
    public List<string> JobDetailGroups { get; } = new();
    public List<MaterialCatalogItem> CatalogItems { get; } = new();
    public List<PlumbingGroupQuote> Quotes { get; } = new();

    private InMemoryDataStore()
    {
        Seed();
    }

    public bool AddCatalogItem(MaterialCatalogItem item, out string message)
    {
        var exists = CatalogItems.Any(x =>
            x.PlumbingGroup.Equals(item.PlumbingGroup, StringComparison.OrdinalIgnoreCase)
            && x.JobDetail.Equals(item.JobDetail, StringComparison.OrdinalIgnoreCase)
            && x.MaterialName.Equals(item.MaterialName, StringComparison.OrdinalIgnoreCase)
            && x.Brand.Equals(item.Brand, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            message = "Aynı tesisat grubu / iş detayı altında aynı malzeme ve marka tekrar eklenemez.";
            return false;
        }

        CatalogItems.Add(item);
        AddPlumbingGroup(item.PlumbingGroup);
        AddJobDetailGroup(item.JobDetail);
        message = "Malzeme başarıyla eklendi.";
        return true;
    }

    public void AddPlumbingGroup(string name)
    {
        if (!string.IsNullOrWhiteSpace(name) && !PlumbingGroups.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            PlumbingGroups.Add(name.Trim());
        }
    }

    public void AddJobDetailGroup(string name)
    {
        if (!string.IsNullOrWhiteSpace(name) && !JobDetailGroups.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            JobDetailGroups.Add(name.Trim());
        }
    }

    private void Seed()
    {
        AddPlumbingGroup("Sıhhi Tesisat");
        AddJobDetailGroup("Vitrifiye");

        AddCatalogItem(new MaterialCatalogItem
        {
            PlumbingGroup = "Sıhhi Tesisat",
            JobDetail = "Vitrifiye",
            MaterialName = "Lavabo",
            Brand = "ECA",
            ListPrice = 10000,
            DiscountPercent = 15
        }, out _);
    }
}
