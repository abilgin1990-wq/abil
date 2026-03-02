using System.Text.Json;

namespace AbilSantiyeTakip;

public class MainForm : Form
{
    private readonly AppData _data = new();
    private string? _currentFile;
    private bool _dirty;

    private readonly TextBox _siteName = new();
    private readonly TextBox _chiefName = new();
    private readonly TextBox _chiefPhone = new();
    private readonly TextBox _workerName = new();
    private readonly TextBox _workerPhone = new();
    private readonly DataGridView _grid = new();

    public MainForm()
    {
        Text = "Şantiye Takip - Ana Sayfa";
        Width = 1200;
        Height = 750;
        StartPosition = FormStartPosition.CenterScreen;

        var menu = BuildMenu();
        MainMenuStrip = menu;
        Controls.Add(menu);

        Controls.Add(BuildInputPanel());
        Controls.Add(BuildGrid());
        RefreshGrid();

        FormClosing += MainForm_FormClosing;
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("Dosya");
        var materialSettings = new ToolStripMenuItem("Malzeme Ekleme Ayar Menüsü", null, (_, _) => OpenMaterialSettings());
        var save = new ToolStripMenuItem("Kaydet", null, (_, _) => SaveData(false));
        var saveAs = new ToolStripMenuItem("Farklı Kaydet", null, (_, _) => SaveData(true));
        var load = new ToolStripMenuItem("Yükle", null, (_, _) => LoadData());
        var exit = new ToolStripMenuItem("Çıkış", null, (_, _) => Close());
        file.DropDownItems.AddRange([materialSettings, save, saveAs, load, exit]);
        menu.Items.Add(file);
        return menu;
    }

    private Panel BuildInputPanel()
    {
        var panel = new Panel { Top = 30, Left = 10, Width = 1160, Height = 130 };

        AddLabeledControl(panel, "Şantiye İsmi", _siteName, 10, 10);
        AddLabeledControl(panel, "Şantiye Şefi", _chiefName, 240, 10);
        AddLabeledControl(panel, "Şantiye Şefi Telefon", _chiefPhone, 470, 10);
        AddLabeledControl(panel, "Usta İsmi", _workerName, 700, 10);
        AddLabeledControl(panel, "Usta Telefon", _workerPhone, 930, 10);

        var btnAdd = new Button { Text = "Ekle", Left = 1020, Top = 80, Width = 120, Height = 30 };
        btnAdd.Click += (_, _) => AddSite();
        panel.Controls.Add(btnAdd);

        return panel;
    }

    private Control BuildGrid()
    {
        _grid.Top = 170;
        _grid.Left = 10;
        _grid.Width = 1160;
        _grid.Height = 520;
        _grid.AllowUserToAddRows = false;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Şantiye İsmi", Width = 420 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Son Güncelleme", Width = 220 });
        _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Detay", Text = "Şantiye Detayını Gör", UseColumnTextForButtonValue = true, Width = 230 });
        _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Text = "Düzenle", UseColumnTextForButtonValue = true, Width = 200 });
        _grid.CellContentClick += Grid_CellContentClick;
        return _grid;
    }

    private static void AddLabeledControl(Control parent, string label, Control ctrl, int x, int y)
    {
        parent.Controls.Add(new Label { Text = label, Left = x, Top = y, Width = 200 });
        ctrl.Left = x;
        ctrl.Top = y + 22;
        ctrl.Width = 210;
        parent.Controls.Add(ctrl);
    }

    private void AddSite()
    {
        if (string.IsNullOrWhiteSpace(_siteName.Text))
        {
            MessageBox.Show("Şantiye ismi zorunludur.");
            return;
        }

        _data.Sites.Add(new ConstructionSite
        {
            SiteName = _siteName.Text.Trim(),
            ChiefName = _chiefName.Text.Trim(),
            ChiefPhone = _chiefPhone.Text.Trim(),
            WorkerName = _workerName.Text.Trim(),
            WorkerPhone = _workerPhone.Text.Trim(),
            LastUpdated = DateTime.Now
        });

        _siteName.Clear();
        _chiefName.Clear();
        _chiefPhone.Clear();
        _workerName.Clear();
        _workerPhone.Clear();

        MarkDirty();
        RefreshGrid();
    }

    private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _data.Sites.Count) return;
        var site = _data.Sites[e.RowIndex];

        if (e.ColumnIndex == 2)
        {
            Hide();
            using var details = new SiteTrackingForm(site, _data.MaterialCatalog, () => MarkDirty()) { Size = Size, StartPosition = FormStartPosition.CenterScreen };
            details.ShowDialog();
            Show();
            RefreshGrid();
        }
        else if (e.ColumnIndex == 3)
        {
            using var dlg = new SiteEditForm(site);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                site.LastUpdated = DateTime.Now;
                MarkDirty();
                RefreshGrid();
            }
        }
    }

    private void RefreshGrid()
    {
        _grid.Rows.Clear();
        foreach (var s in _data.Sites)
        {
            _grid.Rows.Add(s.SiteName, s.LastUpdated.ToString("dd.MM.yyyy HH:mm"));
        }
    }

    private void OpenMaterialSettings()
    {
        using var form = new MaterialSettingsForm(_data.MaterialCatalog);
        if (form.ShowDialog() == DialogResult.OK)
        {
            MarkDirty();
        }
    }

    private void SaveData(bool saveAs)
    {
        if (saveAs || string.IsNullOrWhiteSpace(_currentFile))
        {
            using var sfd = new SaveFileDialog { Filter = "JSON Files (*.json)|*.json", Title = "Veriyi Kaydet" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            _currentFile = sfd.FileName;
        }

        File.WriteAllText(_currentFile!, JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true }));
        _dirty = false;
    }

    private void LoadData()
    {
        using var ofd = new OpenFileDialog { Filter = "JSON Files (*.json)|*.json", Title = "Veri Yükle" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        var loaded = JsonSerializer.Deserialize<AppData>(File.ReadAllText(ofd.FileName));
        if (loaded is null) return;

        _data.Sites.Clear();
        _data.Sites.AddRange(loaded.Sites);
        _data.MaterialCatalog.Clear();
        _data.MaterialCatalog.AddRange(loaded.MaterialCatalog);
        _currentFile = ofd.FileName;
        _dirty = false;
        RefreshGrid();
    }

    private void MarkDirty() => _dirty = true;

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_dirty) return;
        var result = MessageBox.Show("Programda değişiklikler var. Kaydedilsin mi?", "Onay", MessageBoxButtons.YesNoCancel);
        if (result == DialogResult.Cancel)
        {
            e.Cancel = true;
            return;
        }
        if (result == DialogResult.Yes) SaveData(false);
    }
}
