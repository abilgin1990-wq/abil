namespace TeklifHazirlama;

public class SettingsForm : Form
{
    private readonly DataStore _store;

    private readonly TextBox _newGroupText = new() { Width = 180 };
    private readonly ComboBox _groupSelectCombo = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _newWorkDetailText = new() { Width = 180 };
    private readonly ListBox _groupsList = new() { Width = 250, Height = 260 };
    private readonly ListBox _detailsList = new() { Width = 250, Height = 260 };

    private readonly TextBox _usdText = new() { Width = 60, Text = "1" };
    private readonly TextBox _eurText = new() { Width = 60, Text = "1" };
    private readonly ComboBox _matGroupCombo = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _matDetailCombo = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _matNameText = new() { Width = 120 };
    private readonly TextBox _brandText = new() { Width = 120 };
    private readonly TextBox _priceText = new() { Width = 90 };
    private readonly ComboBox _currencyCombo = new() { Width = 70, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _discountText = new() { Width = 60, Text = "0" };
    private readonly TextBox _laborText = new() { Width = 90, Text = "0" };
    private readonly DataGridView _materialsGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };

    public SettingsForm(DataStore store)
    {
        _store = store;
        Text = "Ayarlar";
        Width = 1300;
        Height = 700;

        Controls.Add(BuildTabs());
        ConfigureMaterialsGrid();
        RefreshAll();
    }

    private Control BuildTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };
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

        var groupAddPanel = new FlowLayoutPanel { AutoSize = true };
        var addGroupButton = new Button { Text = "Ekle", Width = 80 };
        addGroupButton.Click += (_, _) => AddGroupTemplate();
        groupAddPanel.Controls.AddRange([new Label { Text = "Tesisat Grubu", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, _newGroupText, addGroupButton]);

        var detailAddPanel = new FlowLayoutPanel { AutoSize = true };
        var addDetailButton = new Button { Text = "İş Detayı Ekle", Width = 100 };
        addDetailButton.Click += (_, _) => AddDetailTemplate();
        detailAddPanel.Controls.AddRange([
            new Label { Text = "Grup", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, _groupSelectCombo,
            new Label { Text = "İş Detayı", AutoSize = true, Padding = new Padding(8, 8, 0, 0) }, _newWorkDetailText,
            addDetailButton
        ]);

        var listPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var deleteButton = new Button { Text = "Sil", Width = 80 };
        var renameButton = new Button { Text = "Düzenle", Width = 80 };
        deleteButton.Click += (_, _) => DeleteTemplateItem();
        renameButton.Click += (_, _) => EditTemplateItem();
        listPanel.Controls.Add(_groupsList);
        listPanel.Controls.Add(_detailsList);
        listPanel.Controls.Add(deleteButton);
        listPanel.Controls.Add(renameButton);

        _groupsList.SelectedIndexChanged += (_, _) => RefreshDetailsList();

        root.Controls.Add(groupAddPanel, 0, 0);
        root.Controls.Add(detailAddPanel, 0, 1);
        root.Controls.Add(listPanel, 0, 2);
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

        var addPanel = new FlowLayoutPanel { AutoSize = true };
        var addButton = new Button { Text = "Ekle", Width = 80 };
        addButton.Click += (_, _) => AddMaterialCatalog();
        addPanel.Controls.AddRange([
            new Label { Text = "Grup", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, _matGroupCombo,
            new Label { Text = "İş Detayı", AutoSize = true, Padding = new Padding(6, 8, 0, 0) }, _matDetailCombo,
            new Label { Text = "Malzeme", AutoSize = true, Padding = new Padding(6, 8, 0, 0) }, _matNameText,
            new Label { Text = "Marka", AutoSize = true, Padding = new Padding(6, 8, 0, 0) }, _brandText,
            new Label { Text = "Fiyat", AutoSize = true, Padding = new Padding(6, 8, 0, 0) }, _priceText,
            _currencyCombo,
            new Label { Text = "İskonto", AutoSize = true, Padding = new Padding(6, 8, 0, 0) }, _discountText,
            new Label { Text = "İşçilik", AutoSize = true, Padding = new Padding(6, 8, 0, 0) }, _laborText,
            addButton
        ]);

        _matGroupCombo.SelectedIndexChanged += (_, _) => RefreshMaterialDetailCombo();

        root.Controls.Add(ratePanel, 0, 0);
        root.Controls.Add(addPanel, 0, 1);
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
        _materialsGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Text = "Düzenle", UseColumnTextForButtonValue = true, Width = 80 });
        _materialsGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", UseColumnTextForButtonValue = true, Width = 80 });

        _materialsGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            var item = (MaterialCatalogItem)_materialsGrid.Rows[e.RowIndex].DataBoundItem;
            if (e.ColumnIndex == 8)
            {
                _matGroupCombo.SelectedItem = item.InstallationGroupName;
                RefreshMaterialDetailCombo();
                _matDetailCombo.SelectedItem = item.WorkDetailName;
                _matNameText.Text = item.MaterialName;
                _brandText.Text = item.Brand;
                _priceText.Text = item.ListPrice.ToString();
                _discountText.Text = item.DiscountPercent.ToString();
                _laborText.Text = item.LaborUnitPrice.ToString();
                _currencyCombo.SelectedItem = item.Currency;
                _store.State.Settings.MaterialCatalog.Remove(item);
            }
            else if (e.ColumnIndex == 9)
            {
                _store.State.Settings.MaterialCatalog.Remove(item);
            }

            _store.MarkDirty();
            RefreshMaterialsGrid();
        };
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
                var index = list.IndexOf(selectedDetail);
                if (index >= 0) list[index] = newName;
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

                var idx = _store.State.Settings.InstallationGroupTemplates.IndexOf(selectedGroup);
                _store.State.Settings.InstallationGroupTemplates[idx] = newName;
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

    private void RefreshMaterialsGrid()
    {
        _materialsGrid.DataSource = null;
        _materialsGrid.DataSource = _store.State.Settings.MaterialCatalog.ToList();
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
        return form.ShowDialog() == DialogResult.OK ? text.Text.Trim() : null;
    }
}
