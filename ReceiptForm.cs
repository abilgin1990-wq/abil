namespace AbilSantiyeTakip;

public class ReceiptForm : Form
{
    private readonly List<string> _catalog;
    private readonly MaterialReceipt _receipt;

    private readonly TextBox _txtName = new();
    private readonly ComboBox _cmbType = new();
    private readonly TextBox _txtQty = new();
    private readonly TextBox _txtAmount = new();
    private readonly DateTimePicker _dtPurchase = new();
    private readonly DataGridView _grid = new();
    private readonly Label _lblTotal = new();
    private readonly Label _lblRows = new();

    public MaterialReceipt? Result { get; private set; }

    public ReceiptForm(List<string> catalog, MaterialReceipt? editing = null)
    {
        _catalog = catalog;
        _receipt = editing ?? new MaterialReceipt { ReceiptNo = GenerateReceiptNo() };

        Text = "Yeni Fiş Ekleme";
        Width = 1100;
        Height = 680;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(new Label { Text = "Malzeme İsmi", Left = 10, Top = 20 });
        _txtName.SetBounds(100, 18, 220, 24);
        var auto = new AutoCompleteStringCollection();
        auto.AddRange(_catalog.ToArray());
        _txtName.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        _txtName.AutoCompleteSource = AutoCompleteSource.CustomSource;
        _txtName.AutoCompleteCustomSource = auto;

        Controls.Add(new Label { Text = "Cins", Left = 340, Top = 20 });
        _cmbType.SetBounds(380, 18, 150, 24);
        _cmbType.Items.AddRange(["Adet", "Torba", "Kg", "Metre", "Paket"]);
        _cmbType.SelectedIndex = 0;

        Controls.Add(new Label { Text = "Adet", Left = 550, Top = 20 });
        _txtQty.SetBounds(590, 18, 100, 24);
        Controls.Add(new Label { Text = "Tutar", Left = 710, Top = 20 });
        _txtAmount.SetBounds(750, 18, 120, 24);

        var btnAdd = new Button { Text = "Ekle", Left = 890, Top = 16, Width = 80 };
        btnAdd.Click += (_, _) => AddRow();
        Controls.AddRange([_txtName, _cmbType, _txtQty, _txtAmount, btnAdd]);

        _grid.SetBounds(10, 60, 1060, 500);
        _grid.AllowUserToAddRows = false;
        _grid.ReadOnly = true;
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stok No", Width = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Malzeme İsmi", Width = 230 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Adet", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Cins", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tutar", Width = 140 });
        Controls.Add(_grid);

        Controls.Add(new Label { Text = "Alınma Tarihi", Left = 760, Top = 570 });
        _dtPurchase.SetBounds(840, 568, 230, 24);
        Controls.Add(_dtPurchase);

        var btnSave = new Button { Text = "Fişi Kaydet", Left = 930, Top = 598, Width = 140 };
        btnSave.Click += (_, _) => SaveReceipt();
        Controls.Add(btnSave);

        _lblRows.SetBounds(10, 598, 350, 24);
        _lblTotal.SetBounds(760, 598, 150, 24);
        Controls.AddRange([_lblRows, _lblTotal]);

        if (editing is not null)
        {
            _dtPurchase.Value = editing.PurchaseDate;
        }

        RefreshGrid();
    }

    private void AddRow()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text)) return;
        if (!int.TryParse(_txtQty.Text, out var qty) || qty <= 0) return;
        if (!decimal.TryParse(_txtAmount.Text, out var amount) || amount < 0) return;

        _receipt.Items.Add(new MaterialReceiptItem
        {
            StockNo = $"STK-{_receipt.Items.Count + 1:000}",
            Name = _txtName.Text.Trim(),
            Quantity = qty,
            Type = _cmbType.Text,
            Amount = amount
        });

        _txtName.Clear(); _txtQty.Clear(); _txtAmount.Clear();
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        _grid.Rows.Clear();
        foreach (var item in _receipt.Items)
            _grid.Rows.Add(item.StockNo, item.Name, item.Quantity, item.Type, item.Amount.ToString("C"));

        _lblRows.Text = $"Eklenen malzeme satırı: {_receipt.Items.Count}";
        _lblTotal.Text = $"Toplam: {_receipt.TotalAmount:C}";
    }

    private void SaveReceipt()
    {
        var supplier = Prompt.Show("Malzemenin Alındığı Yer", "Tedarikçi / Alındığı yer bilgisi");
        if (string.IsNullOrWhiteSpace(supplier)) return;
        if (_receipt.Items.Count == 0)
        {
            MessageBox.Show("En az bir malzeme ekleyin.");
            return;
        }

        _receipt.Supplier = supplier;
        _receipt.PurchaseDate = _dtPurchase.Value.Date;
        Result = _receipt;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static string GenerateReceiptNo()
    {
        var rnd = Random.Shared.Next(100000, 1000000);
        return $"GTS{rnd}";
    }
}
