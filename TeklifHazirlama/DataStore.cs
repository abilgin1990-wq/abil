using System.Text.Json;

namespace TeklifHazirlama;

public class DataStore
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public string? CurrentPath { get; private set; }
    public bool IsDirty { get; private set; }
    public AppState State { get; private set; } = new();

    public void MarkDirty() => IsDirty = true;

    public void Load(string path)
    {
        var json = File.ReadAllText(path);
        State = JsonSerializer.Deserialize<AppState>(json, _jsonOptions) ?? new AppState();
        RefreshCatalogChangeFlags();
        CurrentPath = path;
        IsDirty = false;
    }

    public void RefreshCatalogChangeFlags()
    {
        foreach (var offer in State.Offers)
        {
            foreach (var group in offer.InstallationGroups)
            {
                foreach (var detail in group.WorkDetails)
                {
                    foreach (var material in detail.Materials)
                    {
                        material.IsCatalogOutdated = IsMaterialOutdated(material);
                    }
                }
            }
        }
    }

    private bool IsMaterialOutdated(MaterialSelection material)
    {
        if (material.IsManualOverride)
        {
            return false;
        }

        var catalogItem = State.Settings.MaterialCatalog.FirstOrDefault(item => item.Id == material.MaterialCatalogItemId);
        if (catalogItem == null)
        {
            return true;
        }

        return !string.Equals(material.MaterialName, catalogItem.MaterialName, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(material.Brand, catalogItem.Brand, StringComparison.OrdinalIgnoreCase)
            || material.OriginalListPrice != catalogItem.ListPrice
            || !string.Equals(material.Currency, catalogItem.Currency, StringComparison.OrdinalIgnoreCase)
            || material.DiscountPercent != catalogItem.DiscountPercent
            || material.LaborUnitPrice != catalogItem.LaborUnitPrice;
    }

    public void Save()
    {
        if (string.IsNullOrWhiteSpace(CurrentPath))
        {
            throw new InvalidOperationException("Kayıt yolu bulunamadı.");
        }

        SaveAs(CurrentPath);
    }

    public void SaveAs(string path)
    {
        var json = JsonSerializer.Serialize(State, _jsonOptions);
        File.WriteAllText(path, json);
        CurrentPath = path;
        IsDirty = false;
    }
}
