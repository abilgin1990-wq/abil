namespace TeklifHazirlama;

public class WorkDetailForm : Form
{
    private readonly DataStore _store;
    private readonly Offer _offer;
    private readonly InstallationGroup _group;
    private readonly ComboBox _detailCombo = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDown };
    private readonly DataGridView _detailsGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };
    private readonly Label _materialTotalLabel = new() { AutoSize = true };
    private readonly Label _laborTotalLabel = new() { AutoSize = true };
    private readonly Label _generalTotalLabel = new() { AutoSize = true };

    public WorkDetailForm(DataStore store, Offer offer, InstallationGroup group)
    {
        _store = store;
        _offer = offer;
        _group = group;

        Text = "İş Detayı Sayfası";
        Width = 1000;
        Height = 620;

        Controls.Add(BuildLayout());
        ConfigureGrid();
        RefreshData();
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(new Label
        {
            Text = _group.Name,
            Font = new Font(Font.FontFamily, 12, FontStyle.Bold),
            AutoSize = true,
            Padding = new Padding(0, 8, 0, 8)
        }, 0, 0);

        var addPanel = new FlowLayoutPanel { AutoSize = true };
        var addButton = new Button { Text = "İş Detayı Ekle", Width = 130 };
        addButton.Click += (_, _) => AddDetail();
        addPanel.Controls.AddRange([new Label { Text = "İş Detayı", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, _detailCombo, addButton]);
        root.Controls.Add(addPanel, 0, 1);

        root.Controls.Add(_detailsGrid, 0, 2);

        var totals = new TableLayoutPanel { AutoSize = true, ColumnCount = 1 };
        totals.Controls.Add(_materialTotalLabel);
        totals.Controls.Add(_laborTotalLabel);
        totals.Controls.Add(_generalTotalLabel);
        root.Controls.Add(totals, 0, 3);

        return root;
    }

    private void ConfigureGrid()
    {
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "İş Detayı", DataPropertyName = nameof(WorkDetail.Name), Width = 280 });
        _detailsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Tutar", DataPropertyName = nameof(WorkDetail.GrandTotal), Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _detailsGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Gir", Text = "İş Detayına Gir", UseColumnTextForButtonValue = true, Width = 130 });
        _detailsGrid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "İş Detayını Sil", UseColumnTextForButtonValue = true, Width = 130 });

        _detailsGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            var detail = (WorkDetail)_detailsGrid.Rows[e.RowIndex].DataBoundItem;

            if (e.ColumnIndex == 2)
            {
                using var form = new MaterialListForm(_store, _offer, _group, detail);
                form.ShowDialog();
                _offer.LastUpdated = DateTime.Now;
                _store.MarkDirty();
                RefreshData();
            }
            else if (e.ColumnIndex == 3)
            {
                _group.WorkDetails.RemoveAll(d => d.Id == detail.Id);
                _store.MarkDirty();
                RefreshData();
            }
        };
    }

    private void AddDetail()
    {
        var name = _detailCombo.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        _group.WorkDetails.Add(new WorkDetail { Name = name });
        _offer.LastUpdated = DateTime.Now;
        _store.MarkDirty();
        RefreshData();
    }

    private void RefreshData()
    {
        _detailCombo.Items.Clear();
        if (_store.State.Settings.WorkDetailTemplatesByGroup.TryGetValue(_group.Name, out var details))
        {
            _detailCombo.Items.AddRange(details.Cast<object>().ToArray());
        }

        _detailsGrid.DataSource = null;
        _detailsGrid.DataSource = _group.WorkDetails;

        _materialTotalLabel.Text = $"Malzemelerin Toplamı: {_group.WorkDetails.Sum(x => x.MaterialTotal):N2}";
        _laborTotalLabel.Text = $"İşçiliklerin Toplamı: {_group.WorkDetails.Sum(x => x.LaborTotal):N2}";
        _generalTotalLabel.Text = $"Genel Toplam: {_group.TotalAmount:N2}";
    }
}
