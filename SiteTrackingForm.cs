namespace AbilSantiyeTakip;

public class SiteTrackingForm : Form
{
    private readonly ConstructionSite _site;
    private readonly List<string> _catalog;
    private readonly Action _onChanged;

    private readonly DataGridView _timesheetGrid = new();
    private readonly Label _halfSummary = new();
    private readonly Label _fullSummary = new();

    private readonly DataGridView _receiptGrid = new();
    private readonly Label _receiptTotal = new();

    public SiteTrackingForm(ConstructionSite site, List<string> catalog, Action onChanged)
    {
        _site = site;
        _catalog = catalog;
        _onChanged = onChanged;

        Text = "Şantiye Takip Sayfası";

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildTimesheetTab());
        tabs.TabPages.Add(BuildMaterialTab());
        Controls.Add(tabs);
    }

    private TabPage BuildTimesheetTab()
    {
        var tab = new TabPage("Şantiye Puantaj");
        var btnBack = new Button { Text = "Geri", Left = 10, Top = 10, Width = 80 };
        btnBack.Click += (_, _) => Close();
        tab.Controls.Add(btnBack);

        tab.Controls.Add(BuildInfoPanel(10, 45));

        var dt = new DateTimePicker { Left = 10, Top = 150, Width = 180 };
        var txtCount = new TextBox { Left = 200, Top = 150, Width = 120 };
        var cmb = new ComboBox { Left = 330, Top = 150, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        cmb.Items.AddRange(["Tam Gün", "Yarım Gün"]); cmb.SelectedIndex = 0;
        var btnAdd = new Button { Text = "Ekle", Left = 480, Top = 148, Width = 100 };
        btnAdd.Click += (_, _) =>
        {
            if (!int.TryParse(txtCount.Text, out var count) || count <= 0)
            {
                MessageBox.Show("Çalışan sayısı geçersiz.");
                return;
            }
            var date = dt.Value.Date;
            if (_site.Timesheets.Any(x => x.Date.Date == date))
            {
                MessageBox.Show("Aynı güne birden fazla çalışma eklenemez.");
                return;
            }
            _site.Timesheets.Add(new TimesheetEntry { Date = date, WorkerCount = count, WorkType = cmb.Text });
            _site.LastUpdated = DateTime.Now;
            _onChanged();
            RefreshTimesheets();
        };
        tab.Controls.AddRange([dt, txtCount, cmb, btnAdd]);

        _timesheetGrid.SetBounds(10, 190, 1120, 380);
        _timesheetGrid.AllowUserToAddRows = false;
        _timesheetGrid.ReadOnly = true;
        _timesheetGrid.AutoGenerateColumns = false;
        _timesheetGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tarih", Width = 180 });
        _timesheetGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Çalışan Sayısı", Width = 180 });
        _timesheetGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Çalışma Detayı", Width = 200 });
        _timesheetGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Text = "Düzenle", UseColumnTextForButtonValue = true, Width = 140 });
        _timesheetGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", UseColumnTextForButtonValue = true, Width = 140 });
        _timesheetGrid.CellContentClick += TimesheetGrid_CellContentClick;
        tab.Controls.Add(_timesheetGrid);

        _halfSummary.SetBounds(10, 585, 600, 25);
        _fullSummary.SetBounds(10, 610, 600, 25);
        tab.Controls.AddRange([_halfSummary, _fullSummary]);
        RefreshTimesheets();
        return tab;
    }

    private TabPage BuildMaterialTab()
    {
        var tab = new TabPage("Şantiyeye Giden Malzeme");
        var btnBack = new Button { Text = "Geri", Left = 10, Top = 10, Width = 80 };
        btnBack.Click += (_, _) => Close();
        var btnAdd = new Button { Text = "Fiş Ekle", Left = 10, Top = 45, Width = 100 };
        btnAdd.Click += (_, _) => AddReceipt();
        var btnDetail = new Button { Text = "Giden Malzemeyi Detaylı Gör", Left = 900, Top = 45, Width = 230 };
        btnDetail.Click += (_, _) => ShowSelectedReceiptDetails();

        tab.Controls.AddRange([btnBack, btnAdd, btnDetail]);
        tab.Controls.Add(BuildInfoPanel(10, 80));

        _receiptGrid.SetBounds(10, 180, 1120, 390);
        _receiptGrid.AllowUserToAddRows = false;
        _receiptGrid.ReadOnly = true;
        _receiptGrid.AutoGenerateColumns = false;
        _receiptGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiş No", Width = 140 });
        _receiptGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Alındığı Yer", Width = 190 });
        _receiptGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tarih", Width = 160 });
        _receiptGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiş Tutarı", Width = 150 });
        _receiptGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Detay", Text = "Detayı Gör", UseColumnTextForButtonValue = true, Width = 130 });
        _receiptGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Text = "Düzenle", UseColumnTextForButtonValue = true, Width = 130 });
        _receiptGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", UseColumnTextForButtonValue = true, Width = 110 });
        _receiptGrid.CellContentClick += ReceiptGrid_CellContentClick;
        tab.Controls.Add(_receiptGrid);

        _receiptTotal.SetBounds(10, 585, 500, 25);
        tab.Controls.Add(_receiptTotal);

        RefreshReceipts();
        return tab;
    }

    private Panel BuildInfoPanel(int x, int y)
    {
        var panel = new Panel { Left = x, Top = y, Width = 800, Height = 90 };
        panel.Controls.AddRange([
            new Label { Text = $"Şantiye: {_site.SiteName}", Left = 0, Top = 0, Width = 760 },
            new Label { Text = $"Şantiye Şefi: {_site.ChiefName} - {_site.ChiefPhone}", Left = 0, Top = 25, Width = 760 },
            new Label { Text = $"Usta: {_site.WorkerName} - {_site.WorkerPhone}", Left = 0, Top = 50, Width = 760 },
        ]);
        return panel;
    }

    private void TimesheetGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _site.Timesheets.Count) return;
        var item = _site.Timesheets.OrderBy(t => t.Date).ToList()[e.RowIndex];
        if (e.ColumnIndex == 3)
        {
            var countText = Prompt.Show("Düzenle", "Çalışan sayısı", item.WorkerCount.ToString());
            if (int.TryParse(countText, out var c) && c > 0) item.WorkerCount = c;
            var typeText = Prompt.Show("Düzenle", "Tam Gün / Yarım Gün", item.WorkType);
            if (!string.IsNullOrWhiteSpace(typeText)) item.WorkType = typeText;
        }
        else if (e.ColumnIndex == 4)
        {
            _site.Timesheets.Remove(item);
        }
        _site.LastUpdated = DateTime.Now;
        _onChanged();
        RefreshTimesheets();
    }

    private void RefreshTimesheets()
    {
        var ordered = _site.Timesheets.OrderBy(t => t.Date).ToList();
        _timesheetGrid.Rows.Clear();
        foreach (var t in ordered) _timesheetGrid.Rows.Add(t.Date.ToString("dd.MM.yyyy"), t.WorkerCount, t.WorkType);

        var halfDays = ordered.Where(x => x.WorkType.Contains("Yarım", StringComparison.OrdinalIgnoreCase)).ToList();
        var fullDays = ordered.Where(x => x.WorkType.Contains("Tam", StringComparison.OrdinalIgnoreCase)).ToList();
        _halfSummary.Text = $"Yarım gün çalışılan gün sayısı: {halfDays.Count} | Toplam yarım gün çalışan: {halfDays.Sum(x => x.WorkerCount)}";
        _fullSummary.Text = $"Tam gün çalışılan gün sayısı: {fullDays.Count} | Toplam tam gün çalışan: {fullDays.Sum(x => x.WorkerCount)}";
    }

    private void AddReceipt(MaterialReceipt? editing = null)
    {
        using var f = new ReceiptForm(_catalog, editing);
        if (f.ShowDialog() != DialogResult.OK || f.Result is null) return;

        if (editing is null)
        {
            _site.Receipts.Add(f.Result);
        }
        _site.LastUpdated = DateTime.Now;
        _onChanged();
        RefreshReceipts();
    }

    private void ReceiptGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        var ordered = _site.Receipts.OrderBy(r => r.PurchaseDate).ToList();
        if (e.RowIndex < 0 || e.RowIndex >= ordered.Count) return;
        var r = ordered[e.RowIndex];

        if (e.ColumnIndex == 4) ShowReceipt(r);
        else if (e.ColumnIndex == 5) AddReceipt(r);
        else if (e.ColumnIndex == 6)
        {
            _site.Receipts.Remove(r);
            _site.LastUpdated = DateTime.Now;
            _onChanged();
            RefreshReceipts();
        }
    }

    private void ShowSelectedReceiptDetails()
    {
        if (_receiptGrid.CurrentCell is null || _receiptGrid.CurrentCell.RowIndex < 0) return;
        var ordered = _site.Receipts.OrderBy(r => r.PurchaseDate).ToList();
        if (_receiptGrid.CurrentCell.RowIndex >= ordered.Count) return;
        ShowReceipt(ordered[_receiptGrid.CurrentCell.RowIndex]);
    }

    private static void ShowReceipt(MaterialReceipt r)
    {
        var rows = r.Items.Select(i => $"{i.StockNo} | {i.Name} | {i.Quantity} | {i.Type} | {i.Amount:C}");
        MessageBox.Show(string.Join(Environment.NewLine, rows), $"Fiş Detayı: {r.ReceiptNo}");
    }

    private void RefreshReceipts()
    {
        var ordered = _site.Receipts.OrderByDescending(r => r.PurchaseDate).ToList();
        _receiptGrid.Rows.Clear();
        foreach (var r in ordered)
            _receiptGrid.Rows.Add(r.ReceiptNo, r.Supplier, r.PurchaseDate.ToString("dd.MM.yyyy"), r.TotalAmount.ToString("C"));

        _receiptTotal.Text = $"Fişlerin Toplam Tutarı: {ordered.Sum(x => x.TotalAmount):C}";
    }
}
