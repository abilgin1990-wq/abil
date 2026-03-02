using System.Text.Json.Serialization;

namespace AbilSantiyeTakip;

public class AppData
{
    public List<ConstructionSite> Sites { get; set; } = [];
    public List<string> MaterialCatalog { get; set; } = [];
}

public class ConstructionSite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SiteName { get; set; } = string.Empty;
    public string ChiefName { get; set; } = string.Empty;
    public string ChiefPhone { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string WorkerPhone { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public List<TimesheetEntry> Timesheets { get; set; } = [];
    public List<MaterialReceipt> Receipts { get; set; } = [];
}

public class TimesheetEntry
{
    public DateTime Date { get; set; }
    public int WorkerCount { get; set; }
    public string WorkType { get; set; } = "Tam Gün";
}

public class MaterialReceipt
{
    public string ReceiptNo { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = DateTime.Now;
    public decimal TotalAmount => Items.Sum(i => i.Amount);
    public List<MaterialReceiptItem> Items { get; set; } = [];
}

public class MaterialReceiptItem
{
    public string StockNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
}
