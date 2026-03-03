using System.Windows.Forms;

namespace ConstructionTracker;

public class SiteTrackingForm : Form
{
    private readonly ConstructionSite _site;
    private readonly AppData _appData;
    private readonly Action _markDirty;

    private readonly DateTimePicker _datePicker = new();
    private readonly TextBox _workerCountTextBox = new();
    private readonly ComboBox _workTypeCombo = new();
    private readonly DataGridView _attendanceGrid = new();
    private readonly Label _halfDaySummaryLabel = new();
    private readonly Label _fullDaySummaryLabel = new();

    private readonly DataGridView _receiptGrid = new();
    private readonly Label _receiptTotalLabel = new();

    public SiteTrackingForm(ConstructionSite site, AppData appData, Size parentSize, Action markDirty)
    {
        _site = site;
        _appData = appData;
        _markDirty = markDirty;

        Text = "Şantiye Takip Sayfası";
        Width = parentSize.Width;
        Height = parentSize.Height;
        StartPosition = FormStartPosition.CenterParent;

        InitializeLayout();
        ConfigureAttendanceGrid();
        ConfigureReceiptGrid();
        RefreshSummary();
        RefreshReceiptTotal();
    }

    private void InitializeLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
        var attendanceTab = new TabPage("Şantiye Puantaj");
        var materialTab = new TabPage("Şantiyeye Giden Malzeme");
        var infoTab = new TabPage("Şantiye Bilgileri");

        attendanceTab.Controls.Add(BuildAttendanceTabContent());
        materialTab.Controls.Add(BuildMaterialTabContent());
        infoTab.Controls.Add(BuildInfoTabContent());

        tabs.TabPages.Add(attendanceTab);
        tabs.TabPages.Add(materialTab);
        tabs.TabPages.Add(infoTab);
        Controls.Add(tabs);
    }

    private Control BuildInfoTabContent()
    {
        var p = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 3 };
        p.Controls.Add(new Label { Text = $"Şantiye İsmi: {_site.Name}", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        p.SetColumnSpan(p.Controls[p.Controls.Count - 1], 2);
        p.Controls.Add(new Label { Text = $"Şantiye Şefi: {_site.SiteManagerName}", AutoSize = true }, 0, 1);
        p.Controls.Add(new Label { Text = $"Telefon: {_site.SiteManagerPhone}", AutoSize = true }, 1, 1);
        p.Controls.Add(new Label { Text = $"Usta: {_site.WorkerName}", AutoSize = true }, 0, 2);
        p.Controls.Add(new Label { Text = $"Telefon: {_site.WorkerPhone}", AutoSize = true }, 1, 2);
        return p;
    }

    private Control BuildAttendanceTabContent()
    {
        var c = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 4 };
        c.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        c.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        c.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        c.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        c.Controls.Add(new Label { Text = $"Şantiye: {_site.Name}", Font = new Font(Font, FontStyle.Bold), AutoSize = true }, 0, 0);

        var input = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
        _datePicker.Format = DateTimePickerFormat.Short;
        _workerCountTextBox.Width = 90;
        _workTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _workTypeCombo.DataSource = Enum.GetValues(typeof(WorkType));
        var add = new Button { Text = "Ekle", AutoSize = true };
        add.Click += AddAttendance;
        input.Controls.AddRange(new Control[] { new Label { Text = "Tarih", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, _datePicker, new Label { Text = "Çalışan", AutoSize = true, Padding = new Padding(12, 8, 0, 0) }, _workerCountTextBox, _workTypeCombo, add });

        _attendanceGrid.Dock = DockStyle.Fill;
        var summary = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, Dock = DockStyle.Bottom };
        summary.Controls.Add(_halfDaySummaryLabel);
        summary.Controls.Add(_fullDaySummaryLabel);

        c.Controls.Add(input, 0, 1);
        c.Controls.Add(_attendanceGrid, 0, 2);
        c.Controls.Add(summary, 0, 3);
        return c;
    }

    private Control BuildMaterialTabContent()
    {
        var c = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 4 };
        c.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        c.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        c.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        c.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        c.Controls.Add(new Label { Text = $"Şantiye: {_site.Name}", Font = new Font(Font, FontStyle.Bold), AutoSize = true }, 0, 0);

        var top = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Top, AutoSize = true };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        var addReceipt = new Button { Text = "Fiş Ekle", AutoSize = true, Anchor = AnchorStyles.Left };
        addReceipt.Click += (_, _) => OpenReceiptForm(null);
        var details = new Button { Text = "Giden Malzemeyi Detaylı Gör", AutoSize = true, Anchor = AnchorStyles.Right };
        details.Click += (_, _) => ShowReceiptDetails();
        top.Controls.Add(addReceipt, 0, 0);
        top.Controls.Add(details, 1, 0);

        _receiptGrid.Dock = DockStyle.Fill;
        c.Controls.Add(top, 0, 1);
        c.Controls.Add(_receiptGrid, 0, 2);
        c.Controls.Add(_receiptTotalLabel, 0, 3);
        return c;
    }

    private void ConfigureAttendanceGrid()
    {
        _attendanceGrid.AutoGenerateColumns = false;
        _attendanceGrid.AllowUserToAddRows = false;
        _attendanceGrid.ReadOnly = true;
        _attendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tarih", DataPropertyName = nameof(AttendanceRecord.Date), DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" } });
        _attendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Çalışan Sayısı", DataPropertyName = nameof(AttendanceRecord.WorkerCount) });
        _attendanceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Çalışma Detayı", DataPropertyName = nameof(AttendanceRecord.WorkType) });
        _attendanceGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Text = "Düzenle", Name = "EditAttendanceButton", UseColumnTextForButtonValue = true });
        _attendanceGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", Name = "DeleteAttendanceButton", UseColumnTextForButtonValue = true });
        _attendanceGrid.CellContentClick += AttendanceGridCellContentClick;
        _attendanceGrid.DataSource = _site.AttendanceRecords;
    }

    private void ConfigureReceiptGrid()
    {
        _receiptGrid.AutoGenerateColumns = false;
        _receiptGrid.AllowUserToAddRows = false;
        _receiptGrid.ReadOnly = true;
        _receiptGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiş No", DataPropertyName = nameof(Receipt.ReceiptNo), Width = 130 });
        _receiptGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Alındığı Yer", DataPropertyName = nameof(Receipt.Vendor), Width = 140 });
        _receiptGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tarih", DataPropertyName = nameof(Receipt.Date), DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" } });
        _receiptGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tutar", DataPropertyName = nameof(Receipt.TotalAmount), DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _receiptGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Detay", Text = "Detayı Gör", Name = "ReceiptDetailButton", UseColumnTextForButtonValue = true });
        _receiptGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Text = "Düzenle", Name = "ReceiptEditButton", UseColumnTextForButtonValue = true });
        _receiptGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", Name = "ReceiptDeleteButton", UseColumnTextForButtonValue = true });
        _receiptGrid.CellContentClick += ReceiptGridCellContentClick;
        _receiptGrid.DataSource = _site.Receipts;
    }

    private void AddAttendance(object? sender, EventArgs e)
    {
        if (!int.TryParse(_workerCountTextBox.Text.Trim(), out var wc) || wc <= 0) { MessageBox.Show("Geçerli çalışan sayısı girin."); return; }
        var d = _datePicker.Value.Date;
        if (_site.AttendanceRecords.Any(x => x.Date.Date == d))
        {
            var ok = MessageBox.Show("Aynı güne 2. kez puantaj girmek istediğinize eminmisiniz?", "Uyarı", MessageBoxButtons.YesNo);
            if (ok != DialogResult.Yes) return;
        }

        _site.AttendanceRecords.Add(new AttendanceRecord { Date = d, WorkerCount = wc, WorkType = (WorkType)_workTypeCombo.SelectedItem! });
        _site.LastUpdated = DateTime.Now;
        _workerCountTextBox.Clear();
        RefreshSummary();
        _markDirty();
    }

    private void AttendanceGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var record = _site.AttendanceRecords[e.RowIndex];
        var name = _attendanceGrid.Columns[e.ColumnIndex].Name;
        if (name == "DeleteAttendanceButton") { _site.AttendanceRecords.Remove(record); RefreshSummary(); _site.LastUpdated = DateTime.Now; _markDirty(); }
        if (name == "EditAttendanceButton")
        {
            using var dialog = new EditAttendanceForm(record);
            if (dialog.ShowDialog(this) == DialogResult.OK) { _site.LastUpdated = DateTime.Now; _attendanceGrid.Refresh(); RefreshSummary(); _markDirty(); }
        }
    }

    private void ReceiptGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var receipt = _site.Receipts[e.RowIndex];
        var name = _receiptGrid.Columns[e.ColumnIndex].Name;

        if (name == "ReceiptDeleteButton") { _site.Receipts.RemoveAt(e.RowIndex); RefreshReceiptTotal(); _markDirty(); }
        else if (name == "ReceiptEditButton") OpenReceiptForm(receipt);
        else if (name == "ReceiptDetailButton") new ReceiptDetailForm(receipt).ShowDialog(this);
    }

    private void OpenReceiptForm(Receipt? receipt)
    {
        using var form = new ReceiptForm(receipt, _appData, _site);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _site.LastUpdated = DateTime.Now;
            _receiptGrid.Refresh();
            RefreshReceiptTotal();
            _markDirty();
        }
    }

    private void ShowReceiptDetails()
    {
        if (_site.Receipts.Count == 0) { MessageBox.Show("Kayıtlı fiş yok."); return; }
        new ReceiptDetailForm(_site.Receipts.Last()).ShowDialog(this);
    }

    private void RefreshSummary()
    {
        var half = _site.AttendanceRecords.Where(x => x.WorkType == WorkType.YarimGun).ToList();
        var full = _site.AttendanceRecords.Where(x => x.WorkType == WorkType.TamGun).ToList();
        _halfDaySummaryLabel.Text = $"Yarım gün çalışılan gün: {half.Count} | Yarım gün çalışan toplamı: {half.Sum(x => x.WorkerCount)}";
        _fullDaySummaryLabel.Text = $"Tam gün çalışılan gün: {full.Count} | Tam gün çalışan toplamı: {full.Sum(x => x.WorkerCount)}";
    }

    private void RefreshReceiptTotal() => _receiptTotalLabel.Text = $"Fişlerin toplam tutarı: {_site.Receipts.Sum(x => x.TotalAmount):N2}";
}
