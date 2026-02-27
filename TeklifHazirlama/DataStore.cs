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
        CurrentPath = path;
        IsDirty = false;
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
