using TeklifHazirlama.Models;
using TeklifHazirlama.Services;

namespace TeklifHazirlama.Forms;

public sealed class MainForm : Form
{
    private readonly InMemoryDataStore _store = InMemoryDataStore.Instance;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
    private readonly TextBox _txtNewGroup = new() { PlaceholderText = "Yeni Tesisat Grubu" };

    public MainForm()
    {
        Text = "Teklif Hazırlama - Ana Form";
        Width = 1000;
        Height = 650;

        var btnAdd = new Button { Text = "Tesisat Grubu Ekle" };
        btnAdd.Click += (_, _) => AddPlumbingGroup();

        var btnOpen = new Button { Text = "Seçili Gruba Gir" };
        btnOpen.Click += (_, _) => OpenSelectedGroup();

        var btnDelete = new Button { Text = "Sil" };
        btnDelete.Click += (_, _) => DeleteSelectedGroup();

        var btnMaterialManagement = new Button { Text = "Malzeme Yönetimi" };
        btnMaterialManagement.Click += (_, _) =>
        {
            using var form = new MaterialManagementForm();
            form.ShowDialog(this);
            RefreshGrid();
        };

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45 };
        topPanel.Controls.AddRange([_txtNewGroup, btnAdd, btnOpen, btnDelete, btnMaterialManagement]);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PlumbingGroupQuote.Name), HeaderText = "Tesisat Grubu", Width = 240 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(PlumbingGroupQuote.TotalAmount), HeaderText = "Tutar", Width = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DetailText", HeaderText = "Detay", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        Controls.Add(_grid);
        Controls.Add(topPanel);

        RefreshGrid();
    }

    private void AddPlumbingGroup()
    {
        var name = _txtNewGroup.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Tesisat grubu boş olamaz.");
            return;
        }

        if (_store.Quotes.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Bu tesisat grubu zaten var.");
            return;
        }

        _store.AddPlumbingGroup(name);
        _store.Quotes.Add(new PlumbingGroupQuote { Name = name });
        _txtNewGroup.Clear();
        RefreshGrid();
    }

    private void DeleteSelectedGroup()
    {
        if (GetSelectedGroup() is not { } selected)
        {
            MessageBox.Show("Lütfen silmek için bir tesisat grubu seçin.");
            return;
        }

        var answer = MessageBox.Show("Silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer == DialogResult.Yes)
        {
            _store.Quotes.Remove(selected);
            RefreshGrid();
        }
    }

    private void OpenSelectedGroup()
    {
        if (GetSelectedGroup() is not { } selected)
        {
            MessageBox.Show("Lütfen açmak için bir tesisat grubu seçin.");
            return;
        }

        using var form = new PlumbingGroupForm(selected);
        form.ShowDialog(this);
        RefreshGrid();
    }

    private PlumbingGroupQuote? GetSelectedGroup()
    {
        if (_grid.CurrentRow?.DataBoundItem is PlumbingGroupRow row)
        {
            return row.Source;
        }

        return null;
    }

    private void RefreshGrid()
    {
        var rows = _store.Quotes
            .OrderBy(x => x.Name)
            .Select(x => new PlumbingGroupRow
            {
                Source = x,
                Name = x.Name,
                TotalAmount = x.TotalAmount,
                DetailText = $"İş Detayı Sayısı: {x.JobDetails.Count}"
            })
            .ToList();

        _grid.DataSource = rows;
    }

    private sealed class PlumbingGroupRow
    {
        public required PlumbingGroupQuote Source { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public string DetailText { get; init; } = string.Empty;
    }
}
