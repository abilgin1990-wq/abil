namespace TeklifHazirlama;

public class OfferDetailForm : Form
{
    private const string OpenColumnName = "OpenGroupColumn";
    private const string DeleteColumnName = "DeleteGroupColumn";

    private readonly DataStore _store;
    private readonly Offer _offer;
    private readonly ComboBox _groupCombo = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _groupGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };
    private readonly BindingSource _groupBindingSource = new();
    private readonly Label _generalTotalLabel = new() { AutoSize = true };
    private readonly Label _vatLabel = new() { AutoSize = true };
    private readonly Label _withVatLabel = new() { AutoSize = true };

    public OfferDetailForm(DataStore store, Offer offer)
    {
        _store = store;
        _offer = offer;

        Text = "Teklif Ana Kalemleri";
        Width = 1100;
        Height = 650;

        Controls.Add(BuildLayout());
        FileMenuHelper.Attach(this, _store, onSettingsUpdated: RefreshData);
        ConfigureGrid();
        RefreshData();

        ButtonStyler.Apply(this);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var backButton = new Button { Text = "Geri", Width = 80, Height = 30 };
        backButton.Click += (_, _) => Close();

        var title = new Label
        {
            Text = "Teklif Ana Kalemleri",
            Font = new Font(Font.FontFamily, 13, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 8)
        };

        var titlePanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(3, 30, 3, 3) };
        titlePanel.Controls.Add(backButton);
        titlePanel.Controls.Add(title);

        var info = new Label
        {
            Text = $"Firma: {_offer.CompanyName}    Proje: {_offer.ProjectName}",
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8)
        };

        var addPanel = new FlowLayoutPanel { AutoSize = true };
        var addButton = new Button { Text = "Tesisat Grubu Ekle", Width = 160 };
        addButton.Click += (_, _) => AddGroup();
        addPanel.Controls.AddRange([new Label { Text = "Yeni Tesisat Grubu", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, _groupCombo, addButton]);

        var totalPanel = new TableLayoutPanel { AutoSize = true, ColumnCount = 1 };
        totalPanel.Controls.Add(_generalTotalLabel);
        totalPanel.Controls.Add(_vatLabel);
        totalPanel.Controls.Add(_withVatLabel);

        root.Controls.Add(titlePanel, 0, 0);
        root.Controls.Add(info, 0, 1);
        root.Controls.Add(addPanel, 0, 2);
        root.Controls.Add(_groupGrid, 0, 3);
        root.Controls.Add(totalPanel, 0, 4);

        return root;
    }

    private void ConfigureGrid()
    {
        _groupGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tesisat Grubu", DataPropertyName = nameof(InstallationGroup.DisplayName), Width = 250 });
        _groupGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tutar", DataPropertyName = nameof(InstallationGroup.TotalAmount), Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _groupGrid.Columns.Add(new DataGridViewButtonColumn { Name = OpenColumnName, HeaderText = "Aç", Text = "Tesisat Grubunu Aç", UseColumnTextForButtonValue = true, Width = 150, FlatStyle = FlatStyle.Flat, DefaultCellStyle = new DataGridViewCellStyle { BackColor = ButtonStyler.PrimaryBlue, ForeColor = Color.White, SelectionBackColor = ButtonStyler.PrimaryBlue, SelectionForeColor = Color.White } });
        _groupGrid.Columns.Add(new DataGridViewButtonColumn { Name = DeleteColumnName, HeaderText = "Sil", Text = "Tesisat Grubunu Sil", UseColumnTextForButtonValue = true, Width = 150, FlatStyle = FlatStyle.Flat, DefaultCellStyle = new DataGridViewCellStyle { BackColor = ButtonStyler.PrimaryBlue, ForeColor = Color.White, SelectionBackColor = ButtonStyler.PrimaryBlue, SelectionForeColor = Color.White } });

        _groupGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (_groupGrid.Rows[e.RowIndex].DataBoundItem is not InstallationGroup group) return;
            var clickedColumn = _groupGrid.Columns[e.ColumnIndex].Name;

            if (clickedColumn == OpenColumnName)
            {
                using var form = new WorkDetailForm(_store, _offer, group)
                {
                    StartPosition = FormStartPosition.Manual,
                    Size = Size,
                    Location = Location
                };

                Hide();
                form.ShowDialog();
                Show();

                _offer.LastUpdated = DateTime.Now;
                _store.MarkDirty();
                RefreshData();
            }
            else if (clickedColumn == DeleteColumnName)
            {
                var confirm = MessageBox.Show("Bu tesisat grubunu silmek istediğinize emin misiniz?", "Tesisat Grubu Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                _offer.InstallationGroups.Remove(group);
                _store.MarkDirty();
                RefreshData();
            }
        };


        _groupGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _groupGrid.EnableHeadersVisualStyles = false;
        _groupGrid.ColumnHeadersDefaultCellStyle.Font = new Font(_groupGrid.Font, FontStyle.Bold);
        _groupGrid.DataSource = _groupBindingSource;
    }

    private void AddGroup()
    {
        if (_groupCombo.SelectedItem is not string name || string.IsNullOrWhiteSpace(name)) return;

        var exists = _offer.InstallationGroups.Any(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            MessageBox.Show("Aynı tesisat grubu bu teklifte zaten mevcut.");
            return;
        }

        _offer.InstallationGroups.Add(new InstallationGroup { Name = name.Trim() });
        _offer.LastUpdated = DateTime.Now;
        _store.MarkDirty();
        RefreshData();
    }

    private void RefreshData()
    {
        _store.RefreshCatalogChangeFlags();
        _groupCombo.Items.Clear();
        _groupCombo.Items.AddRange(_store.State.Settings.InstallationGroupTemplates.Cast<object>().ToArray());
        if (_groupCombo.Items.Count > 0 && _groupCombo.SelectedIndex < 0)
        {
            _groupCombo.SelectedIndex = 0;
        }

        if (!ReferenceEquals(_groupBindingSource.DataSource, _offer.InstallationGroups))
        {
            _groupBindingSource.DataSource = _offer.InstallationGroups;
        }

        _groupBindingSource.ResetBindings(false);

        var general = _offer.TotalAmount;
        var vat = general * 0.20m;

        _generalTotalLabel.Text = $"Genel Toplam: {general:N2}";
        _vatLabel.Text = $"KDV Tutarı (%20): {vat:N2}";
        _withVatLabel.Text = $"KDV Dahil Genel Toplam: {general + vat:N2}";
    }
}
