namespace TeklifHazirlama;

public class SettingsForm : Form
{
    private const string EditMaterialColumnName = "EditMaterialColumn";
    private const string DeleteMaterialColumnName = "DeleteMaterialColumn";

    private readonly DataStore _store;

    private readonly TextBox _newGroupText = new() { Width = 180 };
    private readonly ComboBox _groupSelectCombo = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _newWorkDetailText = new() { Width = 180 };
    private readonly ListBox _groupsList = new() { Width = 250, Height = 240 };
    private readonly ListBox _detailsList = new() { Width = 250, Height = 240 };

    private readonly TextBox _usdText = new() { Width = 60, Text = "1" };
    private readonly TextBox _eurText = new() { Width = 60, Text = "1" };
    private readonly ComboBox _matGroupCombo = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _matDetailCombo = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _matNameText = new() { Width = 120 };
    private readonly TextBox _brandText = new() { Width = 120 };
    private readonly ListBox _materialSuggestions = new() { Width = 120, Height = 110, Visible = false, IntegralHeight = false };
    private readonly ListBox _brandSuggestions = new() { Width = 120, Height = 110, Visible = false, IntegralHeight = false };
    private bool _suppressSuggestionUpdate;
    private readonly TextBox _priceText = new() { Width = 90 };
    private readonly ComboBox _currencyCombo = new() { Width = 90, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _discountText = new() { Width = 60, Text = "0" };
    private readonly TextBox _laborText = new() { Width = 90, Text = "0" };
    private readonly DataGridView _materialsGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };

    public SettingsForm(DataStore store)
    {
        _store = store;
        Text = "Ayarlar";
        Width = 1120;
        Height = 620;

        Controls.Add(BuildTabs());
        ConfigureMaterialsGrid();
        RefreshAll();

        ButtonStyler.Apply(this);
    }

    private Control BuildTabs()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            DrawMode = TabDrawMode.OwnerDrawFixed
        };

        tabs.DrawItem += (_, e) =>
        {
            var page = tabs.TabPages[e.Index];
            var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            var backColor = selected ? ButtonStyler.PrimaryBlue : SystemColors.Control;
            var textColor = selected ? Color.White : SystemColors.ControlText;

            using var brush = new SolidBrush(backColor);
            e.Graphics.FillRectangle(brush, e.Bounds);
            TextRenderer.DrawText(e.Graphics, page.Text, tabs.Font, e.Bounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        tabs.TabPages.Add(BuildGroupDetailTab());
        tabs.TabPages.Add(BuildMaterialTab());
        return tabs;
    }

    private TabPage BuildGroupDetailTab()
    {
        var tab = new TabPage("Tesisat ve İş Detayı Ekleme");
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var groupAddPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        var addGroupRow = new FlowLayoutPanel { AutoSize = true };
        var addGroupButton = new Button { Text = "Tesisat Grubu Ekle", Width = 140 };
        addGroupButton.Click += (_, _) => AddGroupTemplate();
        addGroupRow.Controls.AddRange([_newGroupText, addGroupButton]);
        groupAddPanel.Controls.Add(new Label { Text = "Tesisat Grubu", AutoSize = true });
        groupAddPanel.Controls.Add(addGroupRow);

        var detailPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        var labelRow = new TableLayoutPanel { AutoSize = true, ColumnCount = 2 };
        labelRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        labelRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        labelRow.Controls.Add(new Label { Text = "Tesisat Grubu", AutoSize = true }, 0, 0);
        labelRow.Controls.Add(new Label { Text = "İş Detayı", AutoSize = true }, 1, 0);

        var detailRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        var addDetailButton = new Button { Text = "İş Detayı Ekle", Width = 100 };
        addDetailButton.Click += (_, _) => AddDetailTemplate();
        detailRow.Controls.AddRange([_groupSelectCombo, _newWorkDetailText, addDetailButton]);

        detailPanel.Controls.Add(labelRow);
        detailPanel.Controls.Add(detailRow);

        var listRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, AutoSize = true };
        listRoot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        listRoot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        listRoot.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        listRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        listRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        listRoot.Controls.Add(new Label { Text = "Tesisat Grubu", AutoSize = true }, 0, 0);
        listRoot.Controls.Add(new Label { Text = "İş Detayı", AutoSize = true }, 1, 0);

        var buttonPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(10, 0, 0, 0) };
        var deleteButton = new Button { Text = "Sil", Width = 80 };
        var renameButton = new Button { Text = "Düzenle", Width = 80 };
        deleteButton.Click += (_, _) => DeleteTemplateItem();
        renameButton.Click += (_, _) => EditTemplateItem();
        buttonPanel.Controls.Add(deleteButton);
        buttonPanel.Controls.Add(renameButton);

        listRoot.Controls.Add(_groupsList, 0, 1);
        listRoot.Controls.Add(_detailsList, 1, 1);
        listRoot.Controls.Add(buttonPanel, 2, 1);

        _groupsList.SelectedIndexChanged += (_, _) => RefreshDetailsList();

        root.Controls.Add(groupAddPanel, 0, 0);
        root.Controls.Add(detailPanel, 0, 1);
        root.Controls.Add(listRoot, 0, 2);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildMaterialTab()
    {
        var tab = new TabPage("Malzeme Yönetimi");
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var ratePanel = new FlowLayoutPanel { AutoSize = true };
        ratePanel.Controls.AddRange([
            new Label { Text = "Dolar Kuru", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, _usdText,
            new Label { Text = "Euro Kuru", AutoSize = true, Padding = new Padding(8, 8, 0, 0) }, _eurText
        ]);

        _currencyCombo.Items.AddRange(["TRY", "USD", "EUR"]);
        _currencyCombo.SelectedIndex = 0;

        var addArea = new TableLayoutPanel { AutoSize = true, ColumnCount = 5, RowCount = 4 };
        addArea.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        addArea.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        addArea.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        addArea.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        addArea.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        addArea.Controls.Add(new Label { Text = "Grup", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 0);
        addArea.Controls.Add(new Label { Text = "İş Detayı", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 1, 0);
        addArea.Controls.Add(new Label { Text = "Malzeme", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 2, 0);
        addArea.Controls.Add(new Label { Text = "Marka", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 3, 0);

        addArea.Controls.Add(_matGroupCombo, 0, 1);
        addArea.Controls.Add(_matDetailCombo, 1, 1);
        addArea.Controls.Add(_matNameText, 2, 1);
        addArea.Controls.Add(_brandText, 3, 1);

        addArea.Controls.Add(new Label { Text = "Fiyat", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 0, 2);
        addArea.Controls.Add(new Label { Text = "Para Birimi", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 1, 2);
        addArea.Controls.Add(new Label { Text = "İskonto", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 2, 2);
        addArea.Controls.Add(new Label { Text = "İşçilik", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, 3, 2);

        addArea.Controls.Add(_priceText, 0, 3);
        addArea.Controls.Add(_currencyCombo, 1, 3);
        addArea.Controls.Add(_discountText, 2, 3);
        addArea.Controls.Add(_laborText, 3, 3);

        var addButton = new Button { Text = "Ekle", Width = 80, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
        addButton.Click += (_, _) => AddMaterialCatalog();
        addArea.Controls.Add(addButton, 4, 3);

        var formArea = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        formArea.Controls.Add(addArea);

        var suggestionPanel = new TableLayoutPanel { AutoSize = true, ColumnCount = 4, RowCount = 1 };
        suggestionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        suggestionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        suggestionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        suggestionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        suggestionPanel.Controls.Add(_materialSuggestions, 0, 0);
        suggestionPanel.Controls.Add(_brandSuggestions, 1, 0);
        formArea.Controls.Add(suggestionPanel);

        _matGroupCombo.SelectedIndexChanged += (_, _) => RefreshMaterialDetailCombo();
        _matNameText.TextChanged += (_, _) =>
        {
            if (_suppressSuggestionUpdate) return;
            RefreshMaterialsGrid();
            UpdateMaterialSuggestions();
            UpdateBrandSuggestions();
        };
        _brandText.TextChanged += (_, _) =>
        {
            if (_suppressSuggestionUpdate) return;
            RefreshMaterialsGrid();
            UpdateBrandSuggestions();
        };

        _matNameText.KeyDown += (_, e) => HandleSuggestionKeyDown(e, _materialSuggestions, _matNameText, selected =>
        {
            _matNameText.Text = selected;
            UpdateBrandSuggestions();
        });
        _brandText.KeyDown += (_, e) => HandleSuggestionKeyDown(e, _brandSuggestions, _brandText, selected => _brandText.Text = selected);

        _materialSuggestions.Click += (_, _) => ApplySuggestion(_materialSuggestions, _matNameText, selected =>
        {
            _matNameText.Text = selected;
            UpdateBrandSuggestions();
        });
        _materialSuggestions.DoubleClick += (_, _) => ApplySuggestion(_materialSuggestions, _matNameText, selected =>
        {
            _matNameText.Text = selected;
            UpdateBrandSuggestions();
        });
        _brandSuggestions.Click += (_, _) => ApplySuggestion(_brandSuggestions, _brandText, selected => _brandText.Text = selected);
        _brandSuggestions.DoubleClick += (_, _) => ApplySuggestion(_brandSuggestions, _brandText, selected => _brandText.Text = selected);

        _matNameText.Leave += (_, _) => BeginInvoke(new Action(() => { if (!_materialSuggestions.Focused) _materialSuggestions.Visible = false; }));
        _brandText.Leave += (_, _) => BeginInvoke(new Action(() => { if (!_brandSuggestions.Focused) _brandSuggestions.Visible = false; }));
        _materialSuggestions.Leave += (_, _) => _materialSuggestions.Visible = false;
        _brandSuggestions.Leave += (_, _) => _brandSuggestions.Visible = false;

        root.Controls.Add(ratePanel, 0, 0);
        root.Controls.Add(formArea, 0, 1);
        root.Controls.Add(_materialsGrid, 0, 2);

        tab.Controls.Add(root);
        return tab;
    }

    private void ConfigureMaterialsGrid()
    {
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Grup", DataPropertyName = nameof(MaterialCatalogItem.InstallationGroupName), Width = 120 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İş Detayı", DataPropertyName = nameof(MaterialCatalogItem.WorkDetailName), Width = 120 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Malzeme", DataPropertyName = nameof(MaterialCatalogItem.MaterialName), Width = 140 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Marka", DataPropertyName = nameof(MaterialCatalogItem.Brand), Width = 120 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fiyat", DataPropertyName = nameof(MaterialCatalogItem.ListPrice), Width = 90 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PB", DataPropertyName = nameof(MaterialCatalogItem.Currency), Width = 60 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İskonto", DataPropertyName = nameof(MaterialCatalogItem.DiscountPercent), Width = 70 });
        _materialsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İşçilik", DataPropertyName = nameof(MaterialCatalogItem.LaborUnitPrice), Width = 90 });
        _materialsGrid.Columns.Add(new DataGridViewButtonColumn { Name = EditMaterialColumnName, HeaderText = "Düzenle", Text = "Düzenle", UseColumnTextForButtonValue = true, Width = 80, FlatStyle = FlatStyle.Flat, DefaultCellStyle = new DataGridViewCellStyle { BackColor = ButtonStyler.PrimaryBlue, ForeColor = Color.White, SelectionBackColor = ButtonStyler.PrimaryBlue, SelectionForeColor = Color.White } });
        _materialsGrid.Columns.Add(new DataGridViewButtonColumn { Name = DeleteMaterialColumnName, HeaderText = "Sil", Text = "Sil", UseColumnTextForButtonValue = true, Width = 80, FlatStyle = FlatStyle.Flat, DefaultCellStyle = new DataGridViewCellStyle { BackColor = ButtonStyler.PrimaryBlue, ForeColor = Color.White, SelectionBackColor = ButtonStyler.PrimaryBlue, SelectionForeColor = Color.White } });

        _materialsGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (_materialsGrid.Rows[e.RowIndex].DataBoundItem is not MaterialCatalogItem item) return;
            var clickedColumn = _materialsGrid.Columns[e.ColumnIndex].Name;
            if (clickedColumn == EditMaterialColumnName)
            {
                using var editForm = new MaterialCatalogEditForm(_store.State.Settings, item);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    var updatedItem = editForm.EditedItem;
                    item.InstallationGroupName = updatedItem.InstallationGroupName;
                    item.WorkDetailName = updatedItem.WorkDetailName;
                    item.MaterialName = updatedItem.MaterialName;
                    item.Brand = updatedItem.Brand;
                    item.ListPrice = updatedItem.ListPrice;
                    item.Currency = updatedItem.Currency;
                    item.DiscountPercent = updatedItem.DiscountPercent;
                    item.LaborUnitPrice = updatedItem.LaborUnitPrice;

                    _store.RefreshCatalogChangeFlags();
                    _store.MarkDirty();
                    RefreshMaterialsGrid();
                }
            }
            else if (clickedColumn == DeleteMaterialColumnName)
            {
                _store.State.Settings.MaterialCatalog.Remove(item);
                _store.RefreshCatalogChangeFlags();
                _store.MarkDirty();
                RefreshMaterialsGrid();
            }
        };

        _materialsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _materialsGrid.EnableHeadersVisualStyles = false;
        _materialsGrid.ColumnHeadersDefaultCellStyle.Font = new Font(_materialsGrid.Font, FontStyle.Bold);
    }

    private void AddGroupTemplate()
    {
        var name = _newGroupText.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        if (_store.State.Settings.InstallationGroupTemplates.Contains(name)) return;

        _store.State.Settings.InstallationGroupTemplates.Add(name);
        _store.State.Settings.WorkDetailTemplatesByGroup.TryAdd(name, []);
        _newGroupText.Clear();
        _store.MarkDirty();
        RefreshAll();

    }

    private void AddDetailTemplate()
    {
        if (_groupSelectCombo.SelectedItem is not string groupName) return;
        var detail = _newWorkDetailText.Text.Trim();
        if (string.IsNullOrWhiteSpace(detail)) return;

        if (!_store.State.Settings.WorkDetailTemplatesByGroup.TryGetValue(groupName, out var details))
        {
            details = [];
            _store.State.Settings.WorkDetailTemplatesByGroup[groupName] = details;
        }

        if (!details.Contains(detail))
        {
            details.Add(detail);
            _store.MarkDirty();
        }

        _newWorkDetailText.Clear();
        RefreshDetailsList();
    }

    private void DeleteTemplateItem()
    {
        if (_detailsList.SelectedItem is string selectedDetail && _groupsList.SelectedItem is string group)
        {
            _store.State.Settings.WorkDetailTemplatesByGroup[group].Remove(selectedDetail);
        }
        else if (_groupsList.SelectedItem is string selectedGroup)
        {
            _store.State.Settings.InstallationGroupTemplates.Remove(selectedGroup);
            _store.State.Settings.WorkDetailTemplatesByGroup.Remove(selectedGroup);
        }

        _store.MarkDirty();
        RefreshAll();

    }

    private void EditTemplateItem()
    {
        if (_detailsList.SelectedItem is string selectedDetail && _groupsList.SelectedItem is string group)
        {
            var newName = Prompt.Show("Yeni iş detayı adı", selectedDetail);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                var list = _store.State.Settings.WorkDetailTemplatesByGroup[group];
                _store.State.Settings.WorkDetailTemplatesByGroup[group] = list
                    .Select(item => item == selectedDetail ? newName : item)
                    .ToList();
            }
        }
        else if (_groupsList.SelectedItem is string selectedGroup)
        {
            var newName = Prompt.Show("Yeni tesisat grubu adı", selectedGroup);
            if (!string.IsNullOrWhiteSpace(newName) && selectedGroup != newName)
            {
                var details = _store.State.Settings.WorkDetailTemplatesByGroup[selectedGroup];
                _store.State.Settings.WorkDetailTemplatesByGroup.Remove(selectedGroup);
                _store.State.Settings.WorkDetailTemplatesByGroup[newName] = details;

                _store.State.Settings.InstallationGroupTemplates = _store.State.Settings.InstallationGroupTemplates
                    .Select(item => item == selectedGroup ? newName : item)
                    .ToList();
            }
        }

        _store.MarkDirty();
        RefreshAll();

    }

    private void AddMaterialCatalog()
    {
        if (_matGroupCombo.SelectedItem is not string group || _matDetailCombo.SelectedItem is not string detail) return;

        if (!decimal.TryParse(_usdText.Text, out var usd) || !decimal.TryParse(_eurText.Text, out var eur))
        {
            MessageBox.Show("Kur bilgisi geçersiz.");
            return;
        }

        if (!decimal.TryParse(_priceText.Text, out var price) || !decimal.TryParse(_discountText.Text, out var discount) || !decimal.TryParse(_laborText.Text, out var labor))
        {
            MessageBox.Show("Fiyat/iskonto/işçilik bilgileri geçersiz.");
            return;
        }

        _store.State.Settings.DollarRate = usd;
        _store.State.Settings.EuroRate = eur;

        _store.State.Settings.MaterialCatalog.Add(new MaterialCatalogItem
        {
            InstallationGroupName = group,
            WorkDetailName = detail,
            MaterialName = _matNameText.Text.Trim(),
            Brand = _brandText.Text.Trim(),
            ListPrice = price,
            Currency = _currencyCombo.SelectedItem?.ToString() ?? "TRY",
            DiscountPercent = discount,
            LaborUnitPrice = labor
        });

        _matNameText.Clear();
        _brandText.Clear();
        _priceText.Clear();
        _discountText.Text = "0";
        _laborText.Text = "0";

        _store.RefreshCatalogChangeFlags();
        _store.MarkDirty();
        RefreshMaterialsGrid();
    }

    private void RefreshAll()
    {
        _groupsList.DataSource = null;
        _groupsList.DataSource = _store.State.Settings.InstallationGroupTemplates.ToList();

        _groupSelectCombo.Items.Clear();
        _groupSelectCombo.Items.AddRange(_store.State.Settings.InstallationGroupTemplates.Cast<object>().ToArray());
        if (_groupSelectCombo.Items.Count > 0) _groupSelectCombo.SelectedIndex = 0;

        _matGroupCombo.Items.Clear();
        _matGroupCombo.Items.AddRange(_store.State.Settings.InstallationGroupTemplates.Cast<object>().ToArray());
        if (_matGroupCombo.Items.Count > 0) _matGroupCombo.SelectedIndex = 0;

        RefreshDetailsList();
        RefreshMaterialDetailCombo();
        RefreshMaterialsGrid();

        _usdText.Text = _store.State.Settings.DollarRate.ToString();
        _eurText.Text = _store.State.Settings.EuroRate.ToString();
    }

    private void RefreshDetailsList()
    {
        _detailsList.DataSource = null;
        if (_groupsList.SelectedItem is string group && _store.State.Settings.WorkDetailTemplatesByGroup.TryGetValue(group, out var details))
        {
            _detailsList.DataSource = details.ToList();
        }
    }

    private void RefreshMaterialDetailCombo()
    {
        _matDetailCombo.Items.Clear();
        if (_matGroupCombo.SelectedItem is string group && _store.State.Settings.WorkDetailTemplatesByGroup.TryGetValue(group, out var details))
        {
            _matDetailCombo.Items.AddRange(details.Cast<object>().ToArray());
            if (_matDetailCombo.Items.Count > 0) _matDetailCombo.SelectedIndex = 0;
        }
    }

    private void UpdateMaterialSuggestions()
    {
        var materialFilter = _matNameText.Text.Trim();
        var suggestions = _store.State.Settings.MaterialCatalog
            .Select(x => x.MaterialName)
            .Where(x => string.IsNullOrWhiteSpace(materialFilter) || x.Contains(materialFilter, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        BindSuggestions(_materialSuggestions, suggestions, _matNameText.Focused && !string.IsNullOrWhiteSpace(materialFilter));
    }

    private void UpdateBrandSuggestions()
    {
        var materialFilter = _matNameText.Text.Trim();
        var brandFilter = _brandText.Text.Trim();

        var suggestions = _store.State.Settings.MaterialCatalog
            .Where(x => string.IsNullOrWhiteSpace(materialFilter) || x.MaterialName.Contains(materialFilter, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Brand)
            .Where(x => string.IsNullOrWhiteSpace(brandFilter) || x.Contains(brandFilter, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        BindSuggestions(_brandSuggestions, suggestions, _brandText.Focused && !string.IsNullOrWhiteSpace(brandFilter));
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
            RefreshMaterialsGrid();
        }
        finally
        {
            _suppressSuggestionUpdate = false;
        }
    }

    private void RefreshMaterialsGrid()
    {
        var materialFilter = _matNameText.Text.Trim();
        var brandFilter = _brandText.Text.Trim();

        var filtered = _store.State.Settings.MaterialCatalog
            .Where(item => string.IsNullOrWhiteSpace(materialFilter)
                || item.MaterialName.Contains(materialFilter, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(brandFilter)
                || item.Brand.Contains(brandFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _materialsGrid.DataSource = null;
        _materialsGrid.DataSource = filtered;
    }
}

public static class Prompt
{
    public static string? Show(string title, string value)
    {
        using var form = new Form { Width = 420, Height = 160, Text = title, StartPosition = FormStartPosition.CenterParent };
        var text = new TextBox { Left = 20, Top = 20, Width = 360, Text = value };
        var ok = new Button { Text = "Tamam", Left = 220, Width = 75, Top = 60, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "İptal", Left = 305, Width = 75, Top = 60, DialogResult = DialogResult.Cancel };
        form.Controls.AddRange([text, ok, cancel]);
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        ButtonStyler.Apply(form);
        return form.ShowDialog() == DialogResult.OK ? text.Text.Trim() : null;
    }
}
