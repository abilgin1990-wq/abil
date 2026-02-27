using TeklifHazirlama.Models;
using TeklifHazirlama.Services;

namespace TeklifHazirlama.Forms;

public sealed class MaterialManagementForm : Form
{
    private readonly InMemoryDataStore _store = InMemoryDataStore.Instance;

    private readonly ComboBox _cmbPlumbing = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbJobDetail = new() { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtNewPlumbing = new() { Width = 170, PlaceholderText = "Yeni Tesisat Grubu" };
    private readonly TextBox _txtNewJobDetail = new() { Width = 170, PlaceholderText = "Yeni İş Detayı" };
    private readonly TextBox _txtMaterial = new() { Width = 150, PlaceholderText = "Malzeme" };
    private readonly TextBox _txtBrand = new() { Width = 140, PlaceholderText = "Marka" };
    private readonly NumericUpDown _numListPrice = new() { Width = 120, DecimalPlaces = 2, Minimum = 0, Maximum = 100000000 };
    private readonly NumericUpDown _numDiscount = new() { Width = 80, DecimalPlaces = 2, Minimum = 0, Maximum = 100 };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };

    public MaterialManagementForm()
    {
        Text = "Malzeme Yönetimi";
        Width = 1200;
        Height = 650;

        var btnAddPlumbing = new Button { Text = "Tesisat Grubu Ekle" };
        btnAddPlumbing.Click += (_, _) =>
        {
            _store.AddPlumbingGroup(_txtNewPlumbing.Text);
            _txtNewPlumbing.Clear();
            LoadCombos();
        };

        var btnAddJobDetail = new Button { Text = "İş Detayı Ekle" };
        btnAddJobDetail.Click += (_, _) =>
        {
            _store.AddJobDetailGroup(_txtNewJobDetail.Text);
            _txtNewJobDetail.Clear();
            LoadCombos();
        };

        var btnAddMaterial = new Button { Text = "Malzeme Ekle" };
        btnAddMaterial.Click += (_, _) => AddMaterial();

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 85, AutoScroll = true };
        topPanel.Controls.AddRange([
            new Label { Text = "Tesisat Grubu", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, _cmbPlumbing,
            new Label { Text = "İş Detayı", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, _cmbJobDetail,
            _txtNewPlumbing, btnAddPlumbing, _txtNewJobDetail, btnAddJobDetail,
            _txtMaterial, _txtBrand,
            new Label { Text = "Liste Fiyatı", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, _numListPrice,
            new Label { Text = "İskonto %", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, _numDiscount,
            btnAddMaterial
        ]);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MaterialCatalogItem.PlumbingGroup), HeaderText = "Tesisat Grubu", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MaterialCatalogItem.JobDetail), HeaderText = "İş Detayı", Width = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MaterialCatalogItem.MaterialName), HeaderText = "Malzeme", Width = 130 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MaterialCatalogItem.Brand), HeaderText = "Marka", Width = 100 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MaterialCatalogItem.ListPrice), HeaderText = "Liste Fiyatı", Width = 120 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MaterialCatalogItem.DiscountPercent), HeaderText = "İskonto %", Width = 90 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(MaterialCatalogItem.UnitMaterialPrice), HeaderText = "Birim Fiyat", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        Controls.Add(_grid);
        Controls.Add(topPanel);

        LoadCombos();
        RefreshGrid();
    }

    private void LoadCombos()
    {
        _cmbPlumbing.DataSource = _store.PlumbingGroups.OrderBy(x => x).ToList();
        _cmbJobDetail.DataSource = _store.JobDetailGroups.OrderBy(x => x).ToList();
    }

    private void AddMaterial()
    {
        if (_cmbPlumbing.SelectedItem is not string plumbing || _cmbJobDetail.SelectedItem is not string jobDetail)
        {
            MessageBox.Show("Önce tesisat grubu ve iş detayı seçin.");
            return;
        }

        var material = _txtMaterial.Text.Trim();
        var brand = _txtBrand.Text.Trim();

        if (string.IsNullOrWhiteSpace(material) || string.IsNullOrWhiteSpace(brand))
        {
            MessageBox.Show("Malzeme adı ve marka boş olamaz.");
            return;
        }

        var item = new MaterialCatalogItem
        {
            PlumbingGroup = plumbing,
            JobDetail = jobDetail,
            MaterialName = material,
            Brand = brand,
            ListPrice = _numListPrice.Value,
            DiscountPercent = _numDiscount.Value
        };

        if (!_store.AddCatalogItem(item, out var message))
        {
            MessageBox.Show(message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _txtMaterial.Clear();
        _txtBrand.Clear();
        _numListPrice.Value = 0;
        _numDiscount.Value = 0;
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        _grid.DataSource = _store.CatalogItems
            .OrderBy(x => x.PlumbingGroup)
            .ThenBy(x => x.JobDetail)
            .ThenBy(x => x.MaterialName)
            .ThenBy(x => x.Brand)
            .ToList();
    }
}
