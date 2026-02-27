using TeklifHazirlama.Models;
using TeklifHazirlama.Services;

namespace TeklifHazirlama.Forms;

public sealed class JobDetailForm : Form
{
    private readonly string _plumbingGroupName;
    private readonly JobDetailQuote _jobDetail;
    private readonly InMemoryDataStore _store = InMemoryDataStore.Instance;

    private readonly ComboBox _cmbMaterial = new() { Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _numQuantity = new() { DecimalPlaces = 2, Minimum = 1, Maximum = 100000, Value = 1, Width = 100 };
    private readonly NumericUpDown _numLaborUnit = new() { DecimalPlaces = 2, Minimum = 0, Maximum = 1000000, Width = 140 };
    private readonly Label _lblGeneralTotal = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };

    public JobDetailForm(string plumbingGroupName, JobDetailQuote jobDetail)
    {
        _plumbingGroupName = plumbingGroupName;
        _jobDetail = jobDetail;

        Text = $"İş Detayı - {_jobDetail.Name}";
        Width = 1200;
        Height = 650;

        var btnAdd = new Button { Text = "Malzeme Satırı Ekle" };
        btnAdd.Click += (_, _) => AddLine();

        var btnDelete = new Button { Text = "Seçili Satırı Sil" };
        btnDelete.Click += (_, _) => DeleteLine();

        var btnManage = new Button { Text = "Malzeme Yönetimi" };
        btnManage.Click += (_, _) =>
        {
            using var form = new MaterialManagementForm();
            form.ShowDialog(this);
            LoadMaterialCandidates();
        };

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50 };
        topPanel.Controls.AddRange([
            new Label { Text = "Malzeme/Marka", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, _cmbMaterial,
            new Label { Text = "Adet", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, _numQuantity,
            new Label { Text = "İşçilik Birim Fiyat", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, _numLaborUnit,
            btnAdd, btnDelete, btnManage
        ]);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Material", HeaderText = "Malzeme", Width = 140 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Brand", HeaderText = "Marka", Width = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Adet", Width = 70 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ListPrice", HeaderText = "Liste Fiyatı", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Discount", HeaderText = "İskonto %", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Birim Malzeme Fiyatı", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LaborUnit", HeaderText = "İşçilik Birim", Width = 110 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MaterialTotal", HeaderText = "Malzeme Toplam", Width = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LaborTotal", HeaderText = "İşçilik Toplam", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LineTotal", HeaderText = "Satır Toplam", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
        _lblGeneralTotal.Location = new Point(12, 10);
        bottomPanel.Controls.Add(_lblGeneralTotal);

        Controls.Add(_grid);
        Controls.Add(bottomPanel);
        Controls.Add(topPanel);

        LoadMaterialCandidates();
        RefreshGrid();
    }

    private void LoadMaterialCandidates()
    {
        var matches = _store.CatalogItems
            .Where(x => x.PlumbingGroup.Equals(_plumbingGroupName, StringComparison.OrdinalIgnoreCase)
                        && x.JobDetail.Equals(_jobDetail.Name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.MaterialName)
            .ThenBy(x => x.Brand)
            .ToList();

        _cmbMaterial.DataSource = matches;
        _cmbMaterial.DisplayMember = nameof(MaterialCatalogItem.DisplayText);
    }

    private void AddLine()
    {
        if (_cmbMaterial.SelectedItem is not MaterialCatalogItem selected)
        {
            MessageBox.Show("Bu iş detayı için katalogda malzeme bulunamadı. Önce Malzeme Yönetimi'nden tanımlayın.");
            return;
        }

        _jobDetail.MaterialLines.Add(new QuoteMaterialLine
        {
            CatalogItem = selected,
            Quantity = _numQuantity.Value,
            LaborUnitPrice = _numLaborUnit.Value
        });

        RefreshGrid();
    }

    private void DeleteLine()
    {
        if (_grid.CurrentRow?.DataBoundItem is not QuoteLineRow row)
        {
            MessageBox.Show("Lütfen silmek için bir satır seçin.");
            return;
        }

        _jobDetail.MaterialLines.Remove(row.Source);
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        _grid.DataSource = _jobDetail.MaterialLines.Select(x => new QuoteLineRow
        {
            Source = x,
            Material = x.CatalogItem.MaterialName,
            Brand = x.CatalogItem.Brand,
            Quantity = x.Quantity,
            ListPrice = x.CatalogItem.ListPrice,
            Discount = x.CatalogItem.DiscountPercent,
            UnitPrice = x.UnitMaterialPrice,
            LaborUnit = x.LaborUnitPrice,
            MaterialTotal = x.MaterialTotal,
            LaborTotal = x.LaborTotal,
            LineTotal = x.LineTotal
        }).ToList();

        _lblGeneralTotal.Text = $"Genel Toplam: {_jobDetail.TotalAmount:n2} ₺ | Malzeme: {_jobDetail.MaterialTotal:n2} ₺ | İşçilik: {_jobDetail.LaborTotal:n2} ₺";
    }

    private sealed class QuoteLineRow
    {
        public required QuoteMaterialLine Source { get; init; }
        public string Material { get; init; } = string.Empty;
        public string Brand { get; init; } = string.Empty;
        public decimal Quantity { get; init; }
        public decimal ListPrice { get; init; }
        public decimal Discount { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal LaborUnit { get; init; }
        public decimal MaterialTotal { get; init; }
        public decimal LaborTotal { get; init; }
        public decimal LineTotal { get; init; }
    }
}
