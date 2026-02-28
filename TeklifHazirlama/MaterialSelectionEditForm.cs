namespace TeklifHazirlama;

public class MaterialSelectionEditForm : Form
{
    private readonly SettingsData _settings;
    private readonly string _groupName;
    private readonly string _detailName;

    private readonly ComboBox _materialCombo = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _brandCombo = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _quantityText = new() { Width = 120 };
    private readonly TextBox _listPriceText = new() { Width = 120 };
    private readonly ComboBox _currencyCombo = new() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _discountText = new() { Width = 120 };
    private readonly TextBox _laborText = new() { Width = 120 };

    public MaterialSelection EditedSelection { get; }

    public MaterialSelectionEditForm(SettingsData settings, string groupName, string detailName, MaterialSelection source)
    {
        _settings = settings;
        _groupName = groupName;
        _detailName = detailName;

        EditedSelection = new MaterialSelection
        {
            MaterialCatalogItemId = source.MaterialCatalogItemId,
            MaterialName = source.MaterialName,
            Brand = source.Brand,
            Quantity = source.Quantity,
            ListPrice = source.ListPrice,
            OriginalListPrice = source.OriginalListPrice,
            Currency = source.Currency,
            DiscountPercent = source.DiscountPercent,
            UnitPrice = source.UnitPrice,
            LaborUnitPrice = source.LaborUnitPrice,
            IsCatalogOutdated = source.IsCatalogOutdated,
            IsManualOverride = source.IsManualOverride,
            CatalogSnapshot = source.CatalogSnapshot
        };

        Text = "Malzeme Düzenle";
        Width = 500;
        Height = 430;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(BuildLayout());
        BindData();

        ButtonStyler.Apply(this);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), AutoSize = true, ColumnCount = 2 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var row = 0;
        AddRow(root, "Malzeme", _materialCombo, ref row);
        AddRow(root, "Marka", _brandCombo, ref row);
        AddRow(root, "Adet", _quantityText, ref row);
        AddRow(root, "Liste Fiyatı", _listPriceText, ref row);
        AddRow(root, "Para Birimi", _currencyCombo, ref row);
        AddRow(root, "İskonto", _discountText, ref row);
        AddRow(root, "İşçilik Birim", _laborText, ref row);

        var actions = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var save = new Button { Text = "Kaydet", Width = 100 };
        var cancel = new Button { Text = "İptal", Width = 100 };

        save.Click += (_, _) => SaveAndClose();
        cancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        actions.Controls.Add(save);
        actions.Controls.Add(cancel);

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = string.Empty, AutoSize = true }, 0, row);
        root.Controls.Add(actions, 1, row);

        _materialCombo.SelectedIndexChanged += (_, _) =>
        {
            RefreshBrandCombo();
            ApplyCatalogDefaults();
        };

        _brandCombo.SelectedIndexChanged += (_, _) => ApplyCatalogDefaults();

        return root;
    }

    private static void AddRow(TableLayoutPanel root, string labelText, Control input, ref int row)
    {
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = labelText, AutoSize = true, Padding = new Padding(0, 8, 8, 0) }, 0, row);
        root.Controls.Add(input, 1, row);
        row++;
    }

    private void BindData()
    {
        _currencyCombo.Items.AddRange(["TRY", "USD", "EUR"]);

        RefreshMaterialCombo();
        _materialCombo.SelectedItem = EditedSelection.MaterialName;
        if (_materialCombo.SelectedItem == null && _materialCombo.Items.Count > 0)
        {
            _materialCombo.SelectedIndex = 0;
        }

        RefreshBrandCombo();
        _brandCombo.SelectedItem = EditedSelection.Brand;
        if (_brandCombo.SelectedItem == null && _brandCombo.Items.Count > 0)
        {
            _brandCombo.SelectedIndex = 0;
        }

        _quantityText.Text = EditedSelection.Quantity.ToString();
        _listPriceText.Text = EditedSelection.OriginalListPrice.ToString();
        _discountText.Text = EditedSelection.DiscountPercent.ToString();
        _laborText.Text = EditedSelection.LaborUnitPrice.ToString();

        _currencyCombo.SelectedItem = EditedSelection.Currency;
        if (_currencyCombo.SelectedItem == null)
        {
            _currencyCombo.SelectedItem = "TRY";
        }
    }

    private void RefreshMaterialCombo()
    {
        _materialCombo.Items.Clear();

        var materials = _settings.MaterialCatalog
            .Where(item => string.Equals(item.InstallationGroupName, _groupName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(item.WorkDetailName, _detailName, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.MaterialName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .Cast<object>()
            .ToArray();

        _materialCombo.Items.AddRange(materials);

        if (_materialCombo.Items.Count == 0 || !_materialCombo.Items.Contains(EditedSelection.MaterialName))
        {
            _materialCombo.Items.Add(EditedSelection.MaterialName);
        }
    }

    private void RefreshBrandCombo()
    {
        _brandCombo.Items.Clear();

        var selectedMaterial = _materialCombo.SelectedItem?.ToString() ?? string.Empty;
        var brands = _settings.MaterialCatalog
            .Where(item => string.Equals(item.InstallationGroupName, _groupName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(item.WorkDetailName, _detailName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(item.MaterialName, selectedMaterial, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Brand)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .Cast<object>()
            .ToArray();

        _brandCombo.Items.AddRange(brands);

        if (_brandCombo.Items.Count == 0 || !_brandCombo.Items.Contains(EditedSelection.Brand))
        {
            _brandCombo.Items.Add(EditedSelection.Brand);
        }
    }

    private void ApplyCatalogDefaults()
    {
        var selectedMaterial = _materialCombo.SelectedItem?.ToString() ?? string.Empty;
        var selectedBrand = _brandCombo.SelectedItem?.ToString() ?? string.Empty;

        var item = _settings.MaterialCatalog.FirstOrDefault(x =>
            string.Equals(x.InstallationGroupName, _groupName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.WorkDetailName, _detailName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.MaterialName, selectedMaterial, StringComparison.OrdinalIgnoreCase)
            && string.Equals(x.Brand, selectedBrand, StringComparison.OrdinalIgnoreCase));

        if (item == null) return;

        _listPriceText.Text = item.ListPrice.ToString();
        _currencyCombo.SelectedItem = item.Currency;
        _discountText.Text = item.DiscountPercent.ToString();
        _laborText.Text = item.LaborUnitPrice.ToString();
    }

    private void SaveAndClose()
    {
        var materialName = _materialCombo.SelectedItem?.ToString()?.Trim() ?? string.Empty;
        var brand = _brandCombo.SelectedItem?.ToString()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(materialName) || string.IsNullOrWhiteSpace(brand))
        {
            MessageBox.Show("Malzeme ve marka zorunludur.");
            return;
        }

        if (!decimal.TryParse(_quantityText.Text, out var quantity) || quantity <= 0 ||
            !decimal.TryParse(_listPriceText.Text, out var listPrice) || listPrice < 0 ||
            !decimal.TryParse(_discountText.Text, out var discount) || discount < 0 ||
            !decimal.TryParse(_laborText.Text, out var labor) || labor < 0)
        {
            MessageBox.Show("Adet/fiyat/iskonto/işçilik değerleri geçersiz.");
            return;
        }

        EditedSelection.MaterialName = materialName;
        EditedSelection.Brand = brand;
        EditedSelection.Quantity = quantity;
        EditedSelection.OriginalListPrice = listPrice;
        EditedSelection.Currency = _currencyCombo.SelectedItem?.ToString() ?? "TRY";
        EditedSelection.DiscountPercent = discount;
        EditedSelection.LaborUnitPrice = labor;

        DialogResult = DialogResult.OK;
        Close();
    }
}
