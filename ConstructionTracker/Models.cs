using System.ComponentModel;

namespace ConstructionTracker;

public class AppData
{
    public BindingList<ConstructionSite> Sites { get; set; } = new();
    public BindingList<MaterialItem> Materials { get; set; } = new();
    public BindingList<string> MaterialTypes { get; set; } = new();
    public BindingList<string> Vendors { get; set; } = new();
}

public class ConstructionSite
{
    public string Name { get; set; } = string.Empty;
    public string SiteManagerName { get; set; } = string.Empty;
    public string SiteManagerPhone { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string WorkerPhone { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public BindingList<AttendanceRecord> AttendanceRecords { get; set; } = new();
    public BindingList<Receipt> Receipts { get; set; } = new();
}

public class AttendanceRecord
{
    public DateTime Date { get; set; }
    public int WorkerCount { get; set; }
    public WorkType WorkType { get; set; }
}

public enum WorkType
{
    TamGun,
    YarimGun
}

public class MaterialItem
{
    public string StockNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class Receipt
{
    public string ReceiptNo { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public BindingList<ReceiptLine> Lines { get; set; } = new();
}

public class ReceiptLine
{
    public string StockNumber { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string MaterialType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
