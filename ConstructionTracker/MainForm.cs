using System.Text.Json;
using System.Windows.Forms;

namespace ConstructionTracker;

public class MainForm : Form
{
    private AppData _appData = new();
    private string? _currentFilePath;
    private bool _isDirty;

    private readonly TextBox _siteNameTextBox = new();
    private readonly TextBox _managerNameTextBox = new();
    private readonly TextBox _managerPhoneTextBox = new();
    private readonly TextBox _workerNameTextBox = new();
    private readonly TextBox _workerPhoneTextBox = new();
    private readonly DateTimePicker _startDatePicker = new();
    private readonly Button _addButton = new();
    private readonly DataGridView _siteGrid = new();

    public MainForm()
    {
        Text = "Şantiye Yönetimi";
        Width = 1250;
        Height = 760;
        StartPosition = FormStartPosition.CenterScreen;

        BuildMenu();
        InitializeLayout();
        ConfigureGrid();
        BindData();

        FormClosing += MainFormClosing;
    }

    private void BuildMenu()
    {
        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("Dosya");
        var settings = new ToolStripMenuItem("Ayarlar", null, (_, _) => OpenSettings());
        var save = new ToolStripMenuItem("Kaydet", null, (_, _) => SaveData(false));
        var saveAs = new ToolStripMenuItem("Farklı Kaydet", null, (_, _) => SaveData(true));
        var load = new ToolStripMenuItem("Yükle", null, (_, _) => LoadData());
        var exit = new ToolStripMenuItem("Çıkış", null, (_, _) => Close());

        fileMenu.DropDownItems.Add(settings);
        fileMenu.DropDownItems.Add(save);
        fileMenu.DropDownItems.Add(saveAs);
        fileMenu.DropDownItems.Add(load);
        fileMenu.DropDownItems.Add(exit);
        menu.Items.Add(fileMenu);

        MainMenuStrip = menu;
        Controls.Add(menu);
    }

    private void InitializeLayout()
    {
        var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var formPanel = new TableLayoutPanel { AutoSize = true, ColumnCount = 7, Dock = DockStyle.Top };
        for (var i = 0; i < 7; i++) formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        AddLabeledControl(formPanel, "Şantiye İsmi", _siteNameTextBox, 0);
        AddLabeledControl(formPanel, "Şantiye Şefi", _managerNameTextBox, 1);
        AddLabeledControl(formPanel, "Şantiye Şefi Telefon", _managerPhoneTextBox, 2);
        AddLabeledControl(formPanel, "Usta İsmi", _workerNameTextBox, 3);
        AddLabeledControl(formPanel, "Usta Telefon", _workerPhoneTextBox, 4);
        AddLabeledControl(formPanel, "Başlangıç Tarihi", _startDatePicker, 5);

        _startDatePicker.Format = DateTimePickerFormat.Short;
        _addButton.Text = "Ekle";
        _addButton.AutoSize = true;
        _addButton.Click += AddSite;
        formPanel.Controls.Add(_addButton, 6, 1);

        _siteGrid.Dock = DockStyle.Fill;
        mainLayout.Controls.Add(formPanel, 0, 0);
        mainLayout.Controls.Add(_siteGrid, 0, 1);
        Controls.Add(mainLayout);
    }

    private static void AddLabeledControl(TableLayoutPanel panel, string label, Control control, int column)
    {
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(6, 8, 6, 4) }, column, 0);
        control.Margin = new Padding(6, 0, 6, 8);
        control.Width = 155;
        panel.Controls.Add(control, column, 1);
    }

    private void ConfigureGrid()
    {
        _siteGrid.AutoGenerateColumns = false;
        _siteGrid.AllowUserToAddRows = false;
        _siteGrid.ReadOnly = true;

        _siteGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Şantiye İsmi", DataPropertyName = nameof(ConstructionSite.Name), Width = 190 });
        _siteGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Detay", Name = "DetailButton", Text = "Şantiye Detayını Gör", UseColumnTextForButtonValue = true, Width = 150 });
        _siteGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Başlangıç Tarihi", DataPropertyName = nameof(ConstructionSite.StartDate), Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" } });
        _siteGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Name = "EditButton", Text = "Düzenle", UseColumnTextForButtonValue = true, Width = 90 });
        _siteGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Name = "DeleteButton", Text = "Sil", UseColumnTextForButtonValue = true, Width = 70 });
        _siteGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Son Güncelleme", DataPropertyName = nameof(ConstructionSite.LastUpdated), Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });

        _siteGrid.CellContentClick += SiteGridCellContentClick;
    }

    private void BindData() => _siteGrid.DataSource = _appData.Sites;

    private void AddSite(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_siteNameTextBox.Text)) { MessageBox.Show("Şantiye ismi zorunludur."); return; }

        _appData.Sites.Add(new ConstructionSite
        {
            Name = _siteNameTextBox.Text.Trim(),
            SiteManagerName = _managerNameTextBox.Text.Trim(),
            SiteManagerPhone = _managerPhoneTextBox.Text.Trim(),
            WorkerName = _workerNameTextBox.Text.Trim(),
            WorkerPhone = _workerPhoneTextBox.Text.Trim(),
            StartDate = _startDatePicker.Value.Date,
            LastUpdated = DateTime.Now
        });
        ClearInputs();
        MarkDirty();
    }

    private void ClearInputs()
    {
        _siteNameTextBox.Clear(); _managerNameTextBox.Clear(); _managerPhoneTextBox.Clear(); _workerNameTextBox.Clear(); _workerPhoneTextBox.Clear();
        _startDatePicker.Value = DateTime.Today;
    }

    private void SiteGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var site = _appData.Sites[e.RowIndex];
        var col = _siteGrid.Columns[e.ColumnIndex].Name;

        if (col == "DetailButton")
        {
            using var detail = new SiteTrackingForm(site, _appData, Size, MarkDirty);
            Hide();
            detail.ShowDialog(this);
            Show();
            _siteGrid.Refresh();
        }
        else if (col == "EditButton")
        {
            using var edit = new EditSiteForm(site);
            if (edit.ShowDialog(this) == DialogResult.OK) { site.LastUpdated = DateTime.Now; _siteGrid.Refresh(); MarkDirty(); }
        }
        else if (col == "DeleteButton")
        {
            if (MessageBox.Show("Şantiye silinsin mi?", "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _appData.Sites.RemoveAt(e.RowIndex);
                MarkDirty();
            }
        }
    }

    private void OpenSettings()
    {
        using var form = new SettingsForm(_appData, MarkDirty);
        form.ShowDialog(this);
    }

    private void SaveData(bool forcePath)
    {
        if (forcePath || string.IsNullOrWhiteSpace(_currentFilePath))
        {
            using var sfd = new SaveFileDialog { Filter = "JSON|*.json" };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;
            _currentFilePath = sfd.FileName;
        }

        File.WriteAllText(_currentFilePath!, JsonSerializer.Serialize(_appData, new JsonSerializerOptions { WriteIndented = true }));
        _isDirty = false;
    }

    private void LoadData()
    {
        using var ofd = new OpenFileDialog { Filter = "JSON|*.json" };
        if (ofd.ShowDialog(this) != DialogResult.OK) return;

        var loaded = JsonSerializer.Deserialize<AppData>(File.ReadAllText(ofd.FileName));
        if (loaded == null) return;
        _appData = loaded;
        _currentFilePath = ofd.FileName;
        BindData();
        _isDirty = false;
    }

    private void MainFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_isDirty) return;
        var result = MessageBox.Show("Kaydedilsin mi?", "Çıkış", MessageBoxButtons.YesNoCancel);
        if (result == DialogResult.Cancel) { e.Cancel = true; return; }
        if (result == DialogResult.Yes) SaveData(false);
    }

    private void MarkDirty() => _isDirty = true;
}
