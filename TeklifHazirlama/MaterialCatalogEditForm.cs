namespace TeklifHazirlama;

public class MaterialCatalogEditForm : Form
{
    private readonly SettingsData _settings;
    private readonly ComboBox _groupCombo = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _detailCombo = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _materialText = new() { Width = 180 };
    private readonly TextBox _brandText = new() { Width = 150 };
    private readonly TextBox _priceText = new() { Width = 120 };
    private readonly ComboBox _currencyCombo = new() { Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _discountText = new() { Width = 120 };
    private readonly TextBox _laborText = new() { Width = 120 };

    public MaterialCatalogItem EditedItem { get; }

    public MaterialCatalogEditForm(SettingsData settings, MaterialCatalogItem sourceItem)
    {
        _settings = settings;
        EditedItem = new MaterialCatalogItem
        {
            Id = sourceItem.Id,
            InstallationGroupName = sourceItem.InstallationGroupName,
            WorkDetailName = sourceItem.WorkDetailName,
            MaterialName = sourceItem.MaterialName,
            Brand = sourceItem.Brand,
            ListPrice = sourceItem.ListPrice,
            Currency = sourceItem.Currency,
            DiscountPercent = sourceItem.DiscountPercent,
            LaborUnitPrice = sourceItem.LaborUnitPrice
        };

        Text = "Malzeme Düzenle";
        Width = 520;
        Height = 430;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(BuildLayout());
        LoadCombos();
        BindValues();

        ButtonStyler.Apply(this);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12), AutoSize = true };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var row = 0;
        AddRow(root, "Tesisat Grubu", _groupCombo, ref row);
        AddRow(root, "İş Detayı", _detailCombo, ref row);
        AddRow(root, "Malzeme", _materialText, ref row);
        AddRow(root, "Marka", _brandText, ref row);
        AddRow(root, "Fiyat", _priceText, ref row);
        AddRow(root, "Para Birimi", _currencyCombo, ref row);
        AddRow(root, "İskonto", _discountText, ref row);
        AddRow(root, "İşçilik", _laborText, ref row);

        var buttonPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var saveButton = new Button { Text = "Kaydet", Width = 100 };
        var cancelButton = new Button { Text = "İptal", Width = 100 };

        saveButton.Click += (_, _) => SaveAndClose();
        cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = string.Empty, AutoSize = true }, 0, row);
        root.Controls.Add(buttonPanel, 1, row);

        _groupCombo.SelectedIndexChanged += (_, _) => RefreshDetailCombo();

        return root;
    }

    private static void AddRow(TableLayoutPanel root, string label, Control input, ref int row)
    {
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 8, 8, 0) }, 0, row);
        root.Controls.Add(input, 1, row);
        row++;
    }

    private void LoadCombos()
    {
        _currencyCombo.Items.Clear();
        _currencyCombo.Items.AddRange(["TRY", "USD", "EUR"]);

        _groupCombo.Items.Clear();
        _groupCombo.Items.AddRange(_settings.InstallationGroupTemplates.Cast<object>().ToArray());

        if (_groupCombo.Items.Count == 0 && !string.IsNullOrWhiteSpace(EditedItem.InstallationGroupName))
        {
            _groupCombo.Items.Add(EditedItem.InstallationGroupName);
        }
    }

    private void BindValues()
    {
        _groupCombo.SelectedItem = EditedItem.InstallationGroupName;
        if (_groupCombo.SelectedItem == null && _groupCombo.Items.Count > 0)
        {
            _groupCombo.SelectedIndex = 0;
        }

        RefreshDetailCombo();

        _materialText.Text = EditedItem.MaterialName;
        _brandText.Text = EditedItem.Brand;
        _priceText.Text = EditedItem.ListPrice.ToString();
        _discountText.Text = EditedItem.DiscountPercent.ToString();
        _laborText.Text = EditedItem.LaborUnitPrice.ToString();
        _currencyCombo.SelectedItem = EditedItem.Currency;
        if (_currencyCombo.SelectedItem == null && _currencyCombo.Items.Count > 0)
        {
            _currencyCombo.SelectedIndex = 0;
        }
    }

    private void RefreshDetailCombo()
    {
        _detailCombo.Items.Clear();

        var group = _groupCombo.SelectedItem?.ToString() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(group) && _settings.WorkDetailTemplatesByGroup.TryGetValue(group, out var details))
        {
            _detailCombo.Items.AddRange(details.Cast<object>().ToArray());
        }

        if (_detailCombo.Items.Count == 0 && !string.IsNullOrWhiteSpace(EditedItem.WorkDetailName))
        {
            _detailCombo.Items.Add(EditedItem.WorkDetailName);
        }

        _detailCombo.SelectedItem = EditedItem.WorkDetailName;
        if (_detailCombo.SelectedItem == null && _detailCombo.Items.Count > 0)
        {
            _detailCombo.SelectedIndex = 0;
        }
    }

    private void SaveAndClose()
    {
        var group = _groupCombo.SelectedItem?.ToString()?.Trim() ?? string.Empty;
        var detail = _detailCombo.SelectedItem?.ToString()?.Trim() ?? string.Empty;
        var material = _materialText.Text.Trim();
        var brand = _brandText.Text.Trim();

        if (string.IsNullOrWhiteSpace(group) || string.IsNullOrWhiteSpace(detail) || string.IsNullOrWhiteSpace(material) || string.IsNullOrWhiteSpace(brand))
        {
            MessageBox.Show("Tüm alanları doldurunuz.");
            return;
        }

        if (!decimal.TryParse(_priceText.Text, out var price) ||
            !decimal.TryParse(_discountText.Text, out var discount) ||
            !decimal.TryParse(_laborText.Text, out var labor))
        {
            MessageBox.Show("Fiyat/iskonto/işçilik bilgileri geçersiz.");
            return;
        }

        EditedItem.InstallationGroupName = group;
        EditedItem.WorkDetailName = detail;
        EditedItem.MaterialName = material;
        EditedItem.Brand = brand;
        EditedItem.ListPrice = price;
        EditedItem.Currency = _currencyCombo.SelectedItem?.ToString() ?? "TRY";
        EditedItem.DiscountPercent = discount;
        EditedItem.LaborUnitPrice = labor;

        DialogResult = DialogResult.OK;
        Close();
    }
}
