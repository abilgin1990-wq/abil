using System.ComponentModel;

namespace ConstructionTracker;

public class ConstructionSite
{
    public string Name { get; set; } = string.Empty;
    public string SiteManagerName { get; set; } = string.Empty;
    public string SiteManagerPhone { get; set; } = string.Empty;
    public string WorkerName { get; set; } = string.Empty;
    public string WorkerPhone { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public BindingList<AttendanceRecord> AttendanceRecords { get; } = new();
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
