namespace TeklifHazirlamaDesktop;

public class MainForm : Form
{
    private readonly AppState _state;

    private readonly DataGridView _proposalGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false };
    private readonly DataGridView _groupGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false };
    private readonly DataGridView _detailGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false };
    private readonly DataGridView _materialGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false };

    private readonly TextBox _firmName = new();
    private readonly TextBox _projectName = new();

    private readonly TextBox _groupName = new();
    private readonly ComboBox _detailGroup = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _detailName = new();

    private readonly ComboBox _materialGroup = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _materialDetail = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _materialName = new();
    private readonly TextBox _materialBrand = new();
    private readonly NumericUpDown _listPrice = new() { DecimalPlaces = 2, Maximum = 99999999 };
    private readonly NumericUpDown _discount = new() { DecimalPlaces = 2, Maximum = 100 };
    private readonly NumericUpDown _labor = new() { DecimalPlaces = 2, Maximum = 99999999 };

    public MainForm()
    {
        _state = Storage.Load();
        Text = "Teklif Hazırlama - C# Windows";
        Width = 1200;
        Height = 800;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateProposalsTab());
        tabs.TabPages.Add(CreateCatalogTab());
        tabs.TabPages.Add(CreateMaterialsTab());
        Controls.Add(tabs);

        RefreshAll();
    }

    private TabPage CreateProposalsTab()
    {
        var page = new TabPage("Teklifler");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        top.Controls.AddRange([
            new Label { Text = "Firma" }, _firmName,
            new Label { Text = "Proje" }, _projectName,
            NewButton("Teklif Ekle", (_, _) => AddProposal())
        ]);

        _proposalGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Firma", DataPropertyName = "FirmName", Width = 220 });
        _proposalGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Proje", DataPropertyName = "ProjectName", Width = 220 });
        _proposalGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", UseColumnTextForButtonValue = true, Width = 80 });
        _proposalGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 2) return;
            var p = (Proposal)_proposalGrid.Rows[e.RowIndex].DataBoundItem;
            if (MessageBox.Show("Silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            _state.Proposals.RemoveAll(x => x.Id == p.Id);
            SaveAndRefresh();
        };

        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(_proposalGrid, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateCatalogTab()
    {
        var page = new TabPage("Tesisat ve İş Detayı Grupları");
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 330 };

        var catalogTop = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        catalogTop.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        catalogTop.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var groupPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        groupPanel.Controls.AddRange([new Label { Text = "Tesisat Grubu" }, _groupName, NewButton("Tesisat Grubu Ekle", (_, _) => AddCatalogGroup())]);

        var detailPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        detailPanel.Controls.AddRange([
            new Label { Text = "Tesisat Grubu" }, _detailGroup,
            new Label { Text = "İş Detayı" }, _detailName,
            NewButton("İş Detayı Grubu Ekle", (_, _) => AddCatalogDetail())
        ]);

        catalogTop.Controls.Add(groupPanel, 0, 0);
        catalogTop.Controls.Add(detailPanel, 0, 1);

        var bottom = new SplitContainer { Dock = DockStyle.Fill };
        _groupGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tesisat Grubu", DataPropertyName = "Name", Width = 220 });
        _groupGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", UseColumnTextForButtonValue = true, Width = 80 });
        _groupGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
            var g = (PlumbingGroupCatalog)_groupGrid.Rows[e.RowIndex].DataBoundItem;
            _state.CatalogGroups.RemoveAll(x => x.Id == g.Id);
            _state.CatalogDetails.RemoveAll(x => x.GroupId == g.Id);
            _state.Materials.RemoveAll(x => x.GroupId == g.Id);
            SaveAndRefresh();
        };

        _detailGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tesisat Grubu", DataPropertyName = "GroupName", Width = 200 });
        _detailGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İş Detayı", DataPropertyName = "Name", Width = 220 });
        _detailGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", UseColumnTextForButtonValue = true, Width = 80 });
        _detailGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 2) return;
            var id = _detailGrid.Rows[e.RowIndex].Cells[0].Tag?.ToString();
            if (id is null) return;
            _state.CatalogDetails.RemoveAll(x => x.Id == id);
            _state.Materials.RemoveAll(x => x.JobDetailId == id);
            SaveAndRefresh();
        };

        bottom.Panel1.Controls.Add(_groupGrid);
        bottom.Panel2.Controls.Add(_detailGrid);

        split.Panel1.Controls.Add(catalogTop);
        split.Panel2.Controls.Add(bottom);

        page.Controls.Add(split);
        return page;
    }

    private TabPage CreateMaterialsTab()
    {
        var page = new TabPage("Malzeme Yönetimi");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        top.Controls.AddRange([
            new Label { Text = "Tesisat" }, _materialGroup,
            new Label { Text = "İş Detayı" }, _materialDetail,
            new Label { Text = "Malzeme" }, _materialName,
            new Label { Text = "Marka" }, _materialBrand,
            new Label { Text = "Liste Fiyatı" }, _listPrice,
            new Label { Text = "İskonto" }, _discount,
            new Label { Text = "İşçilik" }, _labor,
            NewButton("Malzeme Ekle", (_, _) => AddMaterial())
        ]);

        _materialGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tesisat", DataPropertyName = "GroupName", Width = 130 });
        _materialGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İş Detayı", DataPropertyName = "DetailName", Width = 130 });
        _materialGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Malzeme", DataPropertyName = "Name", Width = 150 });
        _materialGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Marka", DataPropertyName = "Brand", Width = 130 });
        _materialGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Liste", DataPropertyName = "ListPrice", Width = 90 });
        _materialGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İskonto", DataPropertyName = "Discount", Width = 90 });
        _materialGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İşçilik", DataPropertyName = "Labor", Width = 90 });
        _materialGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", UseColumnTextForButtonValue = true, Width = 70 });
        _materialGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 7) return;
            var id = _materialGrid.Rows[e.RowIndex].Cells[0].Tag?.ToString();
            if (id is null) return;
            _state.Materials.RemoveAll(x => x.Id == id);
            SaveAndRefresh();
        };

        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(_materialGrid, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private void AddProposal()
    {
        if (string.IsNullOrWhiteSpace(_firmName.Text) || string.IsNullOrWhiteSpace(_projectName.Text)) return;
        _state.Proposals.Add(new Proposal { FirmName = _firmName.Text.Trim(), ProjectName = _projectName.Text.Trim() });
        _firmName.Clear();
        _projectName.Clear();
        SaveAndRefresh();
    }

    private void AddCatalogGroup()
    {
        var name = _groupName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        if (_state.CatalogGroups.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
        {
            MessageBox.Show("Aynı isimde tesisat grubu var.");
            return;
        }

        _state.CatalogGroups.Add(new PlumbingGroupCatalog { Name = name });
        _groupName.Clear();
        SaveAndRefresh();
    }

    private void AddCatalogDetail()
    {
        if (_detailGroup.SelectedItem is not PlumbingGroupCatalog group) return;
        var name = _detailName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        if (_state.CatalogDetails.Any(x => x.GroupId == group.Id && x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
        {
            MessageBox.Show("Aynı isimde iş detayı var.");
            return;
        }

        _state.CatalogDetails.Add(new JobDetailCatalog { GroupId = group.Id, Name = name });
        _detailName.Clear();
        SaveAndRefresh();
    }

    private void AddMaterial()
    {
        if (_materialGroup.SelectedItem is not PlumbingGroupCatalog group) return;
        if (_materialDetail.SelectedItem is not JobDetailCatalog detail) return;
        var name = _materialName.Text.Trim();
        var brand = _materialBrand.Text.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(brand)) return;

        if (_state.Materials.Any(x => x.JobDetailId == detail.Id && x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) && x.Brand.Equals(brand, StringComparison.CurrentCultureIgnoreCase)))
        {
            MessageBox.Show("Aynı iş detayı içinde aynı malzeme+marka mevcut.");
            return;
        }

        _state.Materials.Add(new MaterialCatalogItem
        {
            GroupId = group.Id,
            JobDetailId = detail.Id,
            Name = name,
            Brand = brand,
            ListPrice = _listPrice.Value,
            Discount = _discount.Value,
            LaborUnitPrice = _labor.Value
        });

        _materialName.Clear();
        _materialBrand.Clear();
        _listPrice.Value = 0;
        _discount.Value = 0;
        _labor.Value = 0;
        SaveAndRefresh();
    }

    private void SaveAndRefresh()
    {
        Storage.Save(_state);
        RefreshAll();
    }

    private void RefreshAll()
    {
        _proposalGrid.DataSource = null;
        _proposalGrid.DataSource = _state.Proposals.ToList();

        _groupGrid.DataSource = null;
        _groupGrid.DataSource = _state.CatalogGroups.ToList();

        _detailGroup.DataSource = null;
        _detailGroup.DisplayMember = nameof(PlumbingGroupCatalog.Name);
        _detailGroup.DataSource = _state.CatalogGroups.ToList();

        _materialGroup.DataSource = null;
        _materialGroup.DisplayMember = nameof(PlumbingGroupCatalog.Name);
        _materialGroup.DataSource = _state.CatalogGroups.ToList();

        var details = _state.CatalogDetails
            .Select(d => new
            {
                d.Id,
                GroupName = _state.CatalogGroups.FirstOrDefault(g => g.Id == d.GroupId)?.Name ?? "-",
                d.Name
            })
            .ToList();

        _detailGrid.Rows.Clear();
        foreach (var d in details)
        {
            var i = _detailGrid.Rows.Add(d.GroupName, d.Name, "Sil");
            _detailGrid.Rows[i].Cells[0].Tag = d.Id;
        }

        _materialDetail.DataSource = null;
        if (_materialGroup.SelectedItem is PlumbingGroupCatalog selectedGroup)
        {
            _materialDetail.DisplayMember = nameof(JobDetailCatalog.Name);
            _materialDetail.DataSource = _state.CatalogDetails.Where(x => x.GroupId == selectedGroup.Id).ToList();
        }

        _materialGrid.Rows.Clear();
        foreach (var m in _state.Materials)
        {
            var groupName = _state.CatalogGroups.FirstOrDefault(x => x.Id == m.GroupId)?.Name ?? "-";
            var detailName = _state.CatalogDetails.FirstOrDefault(x => x.Id == m.JobDetailId)?.Name ?? "-";
            var i = _materialGrid.Rows.Add(groupName, detailName, m.Name, m.Brand, m.ListPrice, m.Discount, m.LaborUnitPrice, "Sil");
            _materialGrid.Rows[i].Cells[0].Tag = m.Id;
        }

        _materialGroup.SelectedIndexChanged -= MaterialGroupChanged;
        _materialGroup.SelectedIndexChanged += MaterialGroupChanged;
    }

    private void MaterialGroupChanged(object? sender, EventArgs e)
    {
        if (_materialGroup.SelectedItem is not PlumbingGroupCatalog group) return;
        _materialDetail.DataSource = _state.CatalogDetails.Where(x => x.GroupId == group.Id).ToList();
        _materialDetail.DisplayMember = nameof(JobDetailCatalog.Name);
    }

    private static Button NewButton(string text, EventHandler onClick)
    {
        var b = new Button { Text = text, AutoSize = true };
        b.Click += onClick;
        return b;
    }
}
