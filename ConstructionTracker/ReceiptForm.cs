using System.Windows.Forms;

namespace ConstructionTracker;

public class ReceiptForm : Form
{
    private readonly Receipt _receipt;
    private readonly AppData _appData;
    private readonly ConstructionSite _site;

    private readonly TextBox _materialNameText = new();
    private readonly ComboBox _materialTypeCombo = new();
    private readonly TextBox _qtyText = new();
    private readonly TextBox _amountText = new();
    private readonly DataGridView _lineGrid = new();
    private readonly DateTimePicker _receiptDate = new();
    private readonly ComboBox _vendorCombo = new();

    public ReceiptForm(Receipt? receipt, AppData appData, ConstructionSite site)
    {
        _appData = appData;
        _site = site;
        _receipt = receipt ?? new Receipt { ReceiptNo = GenerateReceiptNo(site), Date = DateTime.Today };

        Text = "Yeni Fiş Ekleme";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;

        Build();
        Bind();
    }

    private static string GenerateReceiptNo(ConstructionSite site)
    {
        var rnd = new Random();
        string no;
        do { no = $"GTS{rnd.Next(100000, 999999)}"; } while (site.Receipts.Any(x => x.ReceiptNo == no));
        return no;
    }

    private void Build()
    {
        var l = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 4, ColumnCount = 1 };
        l.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        l.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        l.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        l.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var top = new FlowLayoutPanel { AutoSize = true };
        _materialNameText.Width = 180;
        _materialNameText.TextChanged += (_, _) => SuggestMaterial();
        _materialTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _qtyText.Width = 80;
        _amountText.Width = 100;
        var add = new Button { Text = "Ekle" };
        add.Click += AddLine;

        top.Controls.AddRange(new Control[] { new Label { Text = "Malzeme İsmi" }, _materialNameText, new Label { Text = "Cins" }, _materialTypeCombo, new Label { Text = "Adet" }, _qtyText, new Label { Text = "Tutar" }, _amountText, add });

        _lineGrid.Dock = DockStyle.Fill;
        _lineGrid.AutoGenerateColumns = false;
        _lineGrid.AllowUserToAddRows = false;
        _lineGrid.ReadOnly = true;
        _lineGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stok No", DataPropertyName = nameof(ReceiptLine.StockNumber) });
        _lineGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Malzeme İsmi", DataPropertyName = nameof(ReceiptLine.MaterialName), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        _lineGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Adet", DataPropertyName = nameof(ReceiptLine.Quantity) });
        _lineGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Malzeme Cinsi", DataPropertyName = nameof(ReceiptLine.MaterialType) });

        var bottom = new FlowLayoutPanel { AutoSize = true };
        _receiptDate.Format = DateTimePickerFormat.Short;
        _vendorCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        var save = new Button { Text = "Fişi Kaydet" };
        save.Click += SaveReceipt;
        bottom.Controls.AddRange(new Control[] { new Label { Text = "Fiş Tarihi" }, _receiptDate, new Label { Text = "Nereden Alındı" }, _vendorCombo, save });

        l.Controls.Add(new Label { Text = $"Fiş No: {_receipt.ReceiptNo}", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
        l.Controls.Add(top, 0, 1);
        l.Controls.Add(_lineGrid, 0, 2);
        l.Controls.Add(bottom, 0, 3);
        Controls.Add(l);
    }

    private void Bind()
    {
        _materialTypeCombo.DataSource = _appData.MaterialTypes;
        _vendorCombo.DataSource = _appData.Vendors;
        _lineGrid.DataSource = _receipt.Lines;
        _receiptDate.Value = _receipt.Date == default ? DateTime.Today : _receipt.Date;
        if (!string.IsNullOrWhiteSpace(_receipt.Vendor) && _appData.Vendors.Contains(_receipt.Vendor)) _vendorCombo.SelectedItem = _receipt.Vendor;
    }

    private void SuggestMaterial()
    {
        var q = _materialNameText.Text.Trim();
        if (string.IsNullOrWhiteSpace(q)) return;
        var m = _appData.Materials.FirstOrDefault(x => x.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        if (m != null) _materialNameText.Tag = m;
    }

    private void AddLine(object? sender, EventArgs e)
    {
        if (_materialNameText.Tag is not MaterialItem material)
        {
            material = _appData.Materials.FirstOrDefault(x => x.Name.Equals(_materialNameText.Text.Trim(), StringComparison.OrdinalIgnoreCase))!;
            if (material == null) { MessageBox.Show("Malzeme listeden seçilmeli."); return; }
        }
        if (!int.TryParse(_qtyText.Text.Trim(), out var qty) || qty <= 0) return;
        if (!decimal.TryParse(_amountText.Text.Trim(), out var amount) || amount <= 0) return;

        _receipt.Lines.Add(new ReceiptLine
        {
            StockNumber = material.StockNumber,
            MaterialName = material.Name,
            Quantity = qty,
            MaterialType = _materialTypeCombo.SelectedItem?.ToString() ?? string.Empty,
            Amount = amount
        });

        _materialNameText.Clear(); _materialNameText.Tag = null; _qtyText.Clear(); _amountText.Clear();
    }

    private void SaveReceipt(object? sender, EventArgs e)
    {
        if (_receipt.Lines.Count == 0) { MessageBox.Show("Fiş detayı boş."); return; }
        if (_vendorCombo.SelectedItem == null) { MessageBox.Show("Alındığı yer seçin."); return; }

        _receipt.Date = _receiptDate.Value.Date;
        _receipt.Vendor = _vendorCombo.SelectedItem.ToString()!;
        _receipt.TotalAmount = _receipt.Lines.Sum(x => x.Amount);

        if (!_site.Receipts.Contains(_receipt)) _site.Receipts.Add(_receipt);
        DialogResult = DialogResult.OK;
    }
}
