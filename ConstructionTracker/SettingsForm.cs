using System.Windows.Forms;

namespace ConstructionTracker;

public class SettingsForm : Form
{
    private readonly AppData _data;
    private readonly Action _markDirty;

    private readonly TextBox _materialText = new();
    private readonly DataGridView _materialGrid = new();
    private readonly TextBox _typeText = new();
    private readonly DataGridView _typeGrid = new();
    private readonly TextBox _vendorText = new();
    private readonly DataGridView _vendorGrid = new();

    public SettingsForm(AppData data, Action markDirty)
    {
        _data = data;
        _markDirty = markDirty;
        Text = "Ayarlar";
        Width = 950;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;
        Build();
    }

    private void Build()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(MaterialTab());
        tabs.TabPages.Add(TypeTab());
        tabs.TabPages.Add(VendorTab());
        Controls.Add(tabs);
    }

    private TabPage MaterialTab()
    {
        var tab = new TabPage("Malzeme Ekleme");
        var l = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), RowCount = 2 };
        l.RowStyles.Add(new RowStyle(SizeType.AutoSize)); l.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { AutoSize = true };
        _materialText.Width = 280;
        _materialText.TextChanged += (_, _) => ApplyMaterialFilter();
        var add = new Button { Text = "Ekle" };
        add.Click += AddMaterial;
        top.Controls.AddRange(new Control[] { new Label { Text = "Malzeme", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, _materialText, add });

        _materialGrid.Dock = DockStyle.Fill;
        _materialGrid.AutoGenerateColumns = false;
        _materialGrid.AllowUserToAddRows = false;
        _materialGrid.ReadOnly = true;
        _materialGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stok No", DataPropertyName = nameof(MaterialItem.StockNumber) });
        _materialGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Malzeme İsmi", DataPropertyName = nameof(MaterialItem.Name), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _materialGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Text = "Düzenle", Name = "EditMaterial", UseColumnTextForButtonValue = true });
        _materialGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", Name = "DeleteMaterial", UseColumnTextForButtonValue = true });
        _materialGrid.CellContentClick += MaterialGridCellContentClick;
        _materialGrid.DataSource = _data.Materials;

        l.Controls.Add(top, 0, 0);
        l.Controls.Add(_materialGrid, 0, 1);
        tab.Controls.Add(l);
        return tab;
    }

    private TabPage TypeTab()
    {
        var tab = new TabPage("Malzeme Cinsi Ekleme");
        var l = CreateSimpleListTab(_typeText, _typeGrid, "Malzeme Cinsi", AddType, TypeGridCellContentClick);
        tab.Controls.Add(l);
        return tab;
    }

    private TabPage VendorTab()
    {
        var tab = new TabPage("Malzemenin Alındığı Yer Ekleme");
        var l = CreateSimpleListTab(_vendorText, _vendorGrid, "Alındığı Yer", AddVendor, VendorGridCellContentClick);
        tab.Controls.Add(l);
        return tab;
    }

    private static TableLayoutPanel CreateSimpleListTab(TextBox box, DataGridView grid, string label, EventHandler addHandler, DataGridViewCellEventHandler cellHandler)
    {
        var l = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), RowCount = 2 };
        l.RowStyles.Add(new RowStyle(SizeType.AutoSize)); l.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var top = new FlowLayoutPanel { AutoSize = true };
        box.Width = 280;
        var add = new Button { Text = "Ekle" };
        add.Click += addHandler;
        top.Controls.AddRange(new Control[] { new Label { Text = label, AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, box, add });

        grid.Dock = DockStyle.Fill;
        grid.AutoGenerateColumns = false;
        grid.AllowUserToAddRows = false;
        grid.ReadOnly = true;
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = label, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        grid.Columns[0].DataPropertyName = ".";
        grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Text = "Düzenle", Name = "Edit", UseColumnTextForButtonValue = true });
        grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", Name = "Delete", UseColumnTextForButtonValue = true });
        grid.CellContentClick += cellHandler;

        l.Controls.Add(top, 0, 0);
        l.Controls.Add(grid, 0, 1);
        return l;
    }

    private void AddMaterial(object? sender, EventArgs e)
    {
        var name = _materialText.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        if (_data.Materials.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) { MessageBox.Show("Aynı malzeme ismi var, eklenemez."); return; }

        _data.Materials.Add(new MaterialItem { Name = name, StockNumber = GenerateStockNo() });
        _materialText.Clear();
        _materialGrid.DataSource = _data.Materials;
        _markDirty();
    }

    private string GenerateStockNo()
    {
        var rnd = new Random();
        string no;
        do { no = $"STK{rnd.Next(100000, 999999)}"; } while (_data.Materials.Any(x => x.StockNumber == no));
        return no;
    }

    private void ApplyMaterialFilter()
    {
        var q = _materialText.Text.Trim();
        if (string.IsNullOrWhiteSpace(q)) { _materialGrid.DataSource = _data.Materials; return; }
        _materialGrid.DataSource = new BindingSource(new BindingList<MaterialItem>(_data.Materials.Where(x => x.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList()), null);
    }

    private void MaterialGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var col = _materialGrid.Columns[e.ColumnIndex].Name;
        var current = (MaterialItem)_materialGrid.Rows[e.RowIndex].DataBoundItem!;
        var target = _data.Materials.First(x => x.StockNumber == current.StockNumber);
        if (col == "DeleteMaterial") { _data.Materials.Remove(target); _markDirty(); ApplyMaterialFilter(); }
        if (col == "EditMaterial")
        {
            var val = Microsoft.VisualBasic.Interaction.InputBox("Yeni malzeme adı", "Düzenle", target.Name).Trim();
            if (string.IsNullOrWhiteSpace(val)) return;
            if (_data.Materials.Any(x => x != target && x.Name.Equals(val, StringComparison.OrdinalIgnoreCase))) { MessageBox.Show("Aynı isim var."); return; }
            target.Name = val; _materialGrid.Refresh(); _markDirty();
        }
    }

    private void AddType(object? sender, EventArgs e)
    {
        var t = _typeText.Text.Trim();
        if (string.IsNullOrWhiteSpace(t) || _data.MaterialTypes.Any(x => x.Equals(t, StringComparison.OrdinalIgnoreCase))) return;
        _data.MaterialTypes.Add(t); _typeText.Clear(); _typeGrid.DataSource = _data.MaterialTypes; _markDirty();
    }

    private void TypeGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        HandleSimpleListGrid(_typeGrid, _data.MaterialTypes, e, "Malzeme Cinsi");
    }

    private void AddVendor(object? sender, EventArgs e)
    {
        var t = _vendorText.Text.Trim();
        if (string.IsNullOrWhiteSpace(t) || _data.Vendors.Any(x => x.Equals(t, StringComparison.OrdinalIgnoreCase))) return;
        _data.Vendors.Add(t); _vendorText.Clear(); _vendorGrid.DataSource = _data.Vendors; _markDirty();
    }

    private void VendorGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        HandleSimpleListGrid(_vendorGrid, _data.Vendors, e, "Alındığı Yer");
    }

    private void HandleSimpleListGrid(DataGridView grid, BindingList<string> list, DataGridViewCellEventArgs e, string title)
    {
        if (e.RowIndex < 0) return;
        var val = list[e.RowIndex];
        var col = grid.Columns[e.ColumnIndex].Name;
        if (col == "Delete") { list.RemoveAt(e.RowIndex); _markDirty(); }
        if (col == "Edit")
        {
            var newVal = Microsoft.VisualBasic.Interaction.InputBox($"Yeni {title}", "Düzenle", val).Trim();
            if (!string.IsNullOrWhiteSpace(newVal)) { list[e.RowIndex] = newVal; _markDirty(); }
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _typeGrid.DataSource = _data.MaterialTypes;
        _vendorGrid.DataSource = _data.Vendors;
    }
}
