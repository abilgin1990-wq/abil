namespace TeklifHazirlama;

public class MaterialListForm : Form
{
    private const string UpdateMaterialColumnName = "UpdateMaterialColumn";
    private const string DeleteMaterialColumnName = "DeleteMaterialColumn";

    private readonly DataStore _store;
    private readonly Offer _offer;
    private readonly InstallationGroup _group;
    private readonly WorkDetail _detail;

    private readonly TextBox _materialText = new() { Width = 180 };
    private readonly TextBox _brandText = new() { Width = 140 };
    private readonly TextBox _quantityText = new() { Width = 80, Text = "1" };
    private readonly ListBox _materialSuggestions = new() { Width = 180, Height = 110, Visible = false, IntegralHeight = false };
    private readonly ListBox _brandSuggestions = new() { Width = 140, Height = 110, Visible = false, IntegralHeight = false };
    private bool _suppressSuggestionUpdate;
    private readonly DataGridView _materialsGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };
    private readonly BindingSource _materialsBindingSource = new();
    private readonly Label _materialTotalLabel = new() { AutoSize = true };
    private readonly Label _laborTotalLabel = new() { AutoSize = true };
    private readonly Label _generalTotalLabel = new() { AutoSize = true };

    public MaterialListForm(DataStore store, Offer offer, InstallationGroup group, WorkDetail detail)
    {
        _store = store;
        _offer = offer;
        _group = group;
        _detail = detail;

        Text = "Malzeme Listesi";
        Width = 1450;
        Height = 680;

        Controls.Add(BuildLayout());
        ConfigureGrid();
        InitializeFiltering();
        RefreshData();

        ButtonStyler.Apply(this);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var backButton = new Button { Text = "Geri", Width = 80, Height = 30 };
        backButton.Click += (_, _) => Close();

        var titleLabel = new Label
        {
            Text = _detail.Name,
            Font = new Font(Font.FontFamily, 12, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 8)
        };

        var titlePanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        titlePanel.Controls.Add(backButton);
        titlePanel.Controls.Add(titleLabel);
        root.Controls.Add(titlePanel, 0, 0);

        root.RowCount = 5;
        root.RowStyles.Clear();
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var addButton = new Button { Text = "Ekle", Width = 90 };
        addButton.Click += (_, _) => AddMaterial();

        var addPanel = new TableLayoutPanel { AutoSize = true, ColumnCount = 4, RowCount = 2 };
        addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        addPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        addPanel.Controls.Add(new Label { Text = "Malzeme", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 0);
        addPanel.Controls.Add(new Label { Text = "Marka", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 1, 0);
        addPanel.Controls.Add(new Label { Text = "Adet", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 2, 0);

        addPanel.Controls.Add(_materialText, 0, 1);
        addPanel.Controls.Add(_brandText, 1, 1);
        addPanel.Controls.Add(_quantityText, 2, 1);
        addPanel.Controls.Add(addButton, 3, 1);

        var suggestionPanel = new TableLayoutPanel { AutoSize = true, ColumnCount = 4, RowCount = 1 };
        suggestionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        suggestionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        suggestionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        suggestionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        suggestionPanel.Controls.Add(_materialSuggestions, 0, 0);
        suggestionPanel.Controls.Add(_brandSuggestions, 1, 0);

        root.Controls.Add(addPanel, 0, 1);
        root.Controls.Add(suggestionPanel, 0, 2);
        root.Controls.Add(_materialsGrid, 0, 3);

        var totals = new TableLayoutPanel { AutoSize = true, ColumnCount = 1 };
        totals.Controls.Add(_materialTotalLabel);
        totals.Controls.Add(_laborTotalLabel);
        totals.Controls.Add(_generalTotalLabel);
        root.Controls.Add(totals, 0, 4);

        return root;
    }

    private void ConfigureGrid()
    {
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Malzeme", DataPropertyName = nameof(MaterialSelection.DisplayMaterialName), Width = 140 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Marka", DataPropertyName = nameof(MaterialSelection.Brand), Width = 120 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Adet", DataPropertyName = nameof(MaterialSelection.Quantity), Width = 70 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Liste Fiyatı", DataPropertyName = nameof(MaterialSelection.OriginalListPrice), Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PB", DataPropertyName = nameof(MaterialSelection.Currency), Width = 60 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İskonto %", DataPropertyName = nameof(MaterialSelection.DiscountPercent), Width = 80 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Birim Fiyat", DataPropertyName = nameof(MaterialSelection.UnitPrice), Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İşçilik Birim", DataPropertyName = nameof(MaterialSelection.LaborUnitPrice), Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Malzeme Toplam", DataPropertyName = nameof(MaterialSelection.MaterialTotal), Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İşçilik Toplam", DataPropertyName = nameof(MaterialSelection.LaborTotal), Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Genel Toplam", DataPropertyName = nameof(MaterialSelection.GrandTotal), Width = 120, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _materialsGrid.Columns.Add(new DataGridViewButtonColumn { Name = UpdateMaterialColumnName, HeaderText = "Güncelle", Text = "Güncelle", UseColumnTextForButtonValue = false, Width = 110, FlatStyle = FlatStyle.Flat, DefaultCellStyle = new DataGridViewCellStyle { BackColor = ButtonStyler.PrimaryBlue, ForeColor = Color.White, SelectionBackColor = ButtonStyler.PrimaryBlue, SelectionForeColor = Color.White } });
        _materialsGrid.Columns.Add(new DataGridViewButtonColumn { Name = DeleteMaterialColumnName, HeaderText = "Sil", Text = "Malzemeyi Sil", UseColumnTextForButtonValue = true, Width = 110, FlatStyle = FlatStyle.Flat, DefaultCellStyle = new DataGridViewCellStyle { BackColor = ButtonStyler.PrimaryBlue, ForeColor = Color.White, SelectionBackColor = ButtonStyler.PrimaryBlue, SelectionForeColor = Color.White } });

        _materialsGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (_materialsGrid.Rows[e.RowIndex].DataBoundItem is not MaterialSelection material) return;

            var clickedColumn = _materialsGrid.Columns[e.ColumnIndex].Name;
            if (clickedColumn == UpdateMaterialColumnName)
            {
                if (!material.IsCatalogOutdated) return;

                if (!TryUpdateMaterialFromCatalog(material))
                {
                    MessageBox.Show("Bu malzeme için katalog kaydı bulunamadı.");
                    return;
                }

                _offer.LastUpdated = DateTime.Now;
                _store.MarkDirty();
                RefreshData();
            }
            else if (clickedColumn == DeleteMaterialColumnName)
            {
                var confirm = MessageBox.Show("Bu malzemeyi silmek istediğinize emin misiniz?", "Malzeme Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                _detail.Materials.Remove(material);
                _offer.LastUpdated = DateTime.Now;
                _store.MarkDirty();
                RefreshData();
            }
        };

        _materialText.TextChanged += (_, _) =>
        {
            if (_suppressSuggestionUpdate) return;
            UpdateMaterialSuggestions();
            UpdateBrandSuggestions();
        };
        _brandText.TextChanged += (_, _) =>
        {
            if (_suppressSuggestionUpdate) return;
            UpdateBrandSuggestions();
        };

        _materialText.KeyDown += (_, e) => HandleSuggestionKeyDown(e, _materialSuggestions, _materialText, selected =>
        {
            _materialText.Text = selected;
            UpdateBrandSuggestions();
        });

        _brandText.KeyDown += (_, e) => HandleSuggestionKeyDown(e, _brandSuggestions, _brandText, selected => _brandText.Text = selected);

        _materialSuggestions.DoubleClick += (_, _) => ApplySuggestion(_materialSuggestions, _materialText, selected =>
        {
            _materialText.Text = selected;
            UpdateBrandSuggestions();
        });
        _brandSuggestions.DoubleClick += (_, _) => ApplySuggestion(_brandSuggestions, _brandText, selected => _brandText.Text = selected);

        _materialSuggestions.Click += (_, _) => ApplySuggestion(_materialSuggestions, _materialText, selected =>
        {
            _materialText.Text = selected;
            UpdateBrandSuggestions();
        });
        _brandSuggestions.Click += (_, _) => ApplySuggestion(_brandSuggestions, _brandText, selected => _brandText.Text = selected);

        _materialText.Leave += (_, _) => BeginInvoke(new Action(() => { if (!_materialSuggestions.Focused) _materialSuggestions.Visible = false; }));
        _brandText.Leave += (_, _) => BeginInvoke(new Action(() => { if (!_brandSuggestions.Focused) _brandSuggestions.Visible = false; }));
        _materialSuggestions.Leave += (_, _) => _materialSuggestions.Visible = false;
        _brandSuggestions.Leave += (_, _) => _brandSuggestions.Visible = false;

        _materialsGrid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || _materialsGrid.Columns[e.ColumnIndex].Name != UpdateMaterialColumnName) return;
            if (_materialsGrid.Rows[e.RowIndex].DataBoundItem is not MaterialSelection material) return;

            e.Value = material.IsCatalogOutdated ? "Güncelle" : string.Empty;
            e.FormattingApplied = true;
        };

        _materialsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _materialsGrid.EnableHeadersVisualStyles = false;
        _materialsGrid.ColumnHeadersDefaultCellStyle.Font = new Font(_materialsGrid.Font, FontStyle.Bold);
        _materialsGrid.DataSource = _materialsBindingSource;
    }

    private void InitializeFiltering()
    {
        UpdateMaterialSuggestions();
        UpdateBrandSuggestions();
        _materialSuggestions.Visible = false;
        _brandSuggestions.Visible = false;
    }

    private void UpdateMaterialSuggestions()
    {
        var materialFilter = _materialText.Text.Trim();

        var materials = _store.State.Settings.MaterialCatalog
            .Where(m => m.InstallationGroupName == _group.Name && m.WorkDetailName == _detail.Name)
            .Select(m => m.MaterialName)
            .Where(name => string.IsNullOrWhiteSpace(materialFilter) || name.Contains(materialFilter, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        BindSuggestions(_materialSuggestions, materials, _materialText.Focused && !string.IsNullOrWhiteSpace(materialFilter));
    }

    private void UpdateBrandSuggestions()
    {
        var materialFilter = _materialText.Text.Trim();
        var brandFilter = _brandText.Text.Trim();

        var brands = _store.State.Settings.MaterialCatalog
            .Where(m => m.InstallationGroupName == _group.Name && m.WorkDetailName == _detail.Name)
            .Where(m => string.IsNullOrWhiteSpace(materialFilter) || m.MaterialName.Contains(materialFilter, StringComparison.OrdinalIgnoreCase))
            .Select(m => m.Brand)
            .Where(brand => string.IsNullOrWhiteSpace(brandFilter) || brand.Contains(brandFilter, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        BindSuggestions(_brandSuggestions, brands, _brandText.Focused && !string.IsNullOrWhiteSpace(brandFilter));
    }

    private static void BindSuggestions(ListBox listBox, List<string> items, bool show)
    {
        listBox.BeginUpdate();
        listBox.Items.Clear();
        foreach (var item in items)
        {
            listBox.Items.Add(item);
        }
        listBox.EndUpdate();
        listBox.Visible = show && listBox.Items.Count > 0;
    }

    private void HandleSuggestionKeyDown(KeyEventArgs e, ListBox listBox, TextBox targetTextBox, Action<string> apply)
    {
        if (!listBox.Visible || listBox.Items.Count == 0) return;

        if (e.KeyCode == Keys.Down)
        {
            listBox.Focus();
            listBox.SelectedIndex = 0;
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.Enter)
        {
            ApplySuggestion(listBox, targetTextBox, apply);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }

    private void ApplySuggestion(ListBox listBox, TextBox targetTextBox, Action<string> apply)
    {
        if (listBox.SelectedItem is not string selected) return;

        _suppressSuggestionUpdate = true;
        try
        {
            apply(selected);
            targetTextBox.SelectionStart = targetTextBox.TextLength;
            targetTextBox.SelectionLength = 0;
            listBox.Visible = false;
            targetTextBox.Focus();
        }
        finally
        {
            _suppressSuggestionUpdate = false;
        }
    }

    private bool TryUpdateMaterialFromCatalog(MaterialSelection material)
    {
        var item = _store.State.Settings.MaterialCatalog.FirstOrDefault(x => x.Id == material.MaterialCatalogItemId);
        if (item == null) return false;

        ApplyCatalogItem(material, item);
        return true;
    }

    private void ApplyCatalogItem(MaterialSelection target, MaterialCatalogItem item)
    {
        var rate = item.Currency switch
        {
            "USD" => _store.State.Settings.DollarRate,
            "EUR" => _store.State.Settings.EuroRate,
            _ => 1m
        };

        target.MaterialName = item.MaterialName;
        target.Brand = item.Brand;
        target.OriginalListPrice = item.ListPrice;
        target.Currency = item.Currency;
        target.ListPrice = item.ListPrice * rate;
        target.DiscountPercent = item.DiscountPercent;
        target.UnitPrice = item.UnitPrice * rate;
        target.LaborUnitPrice = item.LaborUnitPrice;
        target.IsCatalogOutdated = false;
    }

    private void AddMaterial()
    {
        var materialName = _materialText.Text.Trim();
        var brand = _brandText.Text.Trim();
        if (string.IsNullOrWhiteSpace(materialName) || string.IsNullOrWhiteSpace(brand))
        {
            MessageBox.Show("Malzeme ve marka giriniz.");
            return;
        }

        if (!decimal.TryParse(_quantityText.Text, out var quantity) || quantity <= 0)
        {
            MessageBox.Show("Geçerli bir adet giriniz.");
            return;
        }

        var duplicate = _detail.Materials.Any(m =>
            string.Equals(m.MaterialName, materialName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Brand, brand, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            MessageBox.Show("Aynı malzeme ve marka zaten listede mevcut.");
            return;
        }

        var item = _store.State.Settings.MaterialCatalog.FirstOrDefault(m =>
            m.InstallationGroupName == _group.Name &&
            m.WorkDetailName == _detail.Name &&
            string.Equals(m.MaterialName, materialName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Brand, brand, StringComparison.OrdinalIgnoreCase));

        if (item == null)
        {
            MessageBox.Show("Girilen malzeme/marka için katalog kaydı bulunamadı.");
            return;
        }

        var target = new MaterialSelection
        {
            MaterialCatalogItemId = item.Id,
            Quantity = quantity
        };

        ApplyCatalogItem(target, item);
        _detail.Materials.Add(target);

        _offer.LastUpdated = DateTime.Now;
        _store.MarkDirty();
        RefreshData();
    }

    private void RefreshData()
    {
        _store.RefreshCatalogChangeFlags();

        if (!ReferenceEquals(_materialsBindingSource.DataSource, _detail.Materials))
        {
            _materialsBindingSource.DataSource = _detail.Materials;
        }

        _materialsBindingSource.ResetBindings(false);

        _materialTotalLabel.Text = $"Malzemelerin Toplam Tutarı: {_detail.MaterialTotal:N2}";
        _laborTotalLabel.Text = $"İşçilik Toplam Tutarı: {_detail.LaborTotal:N2}";
        _generalTotalLabel.Text = $"Genel Toplam: {_detail.GrandTotal:N2}";
    }
}
