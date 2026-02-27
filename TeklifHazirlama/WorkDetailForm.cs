namespace TeklifHazirlama;

public class WorkDetailForm : Form
{
    private const string EnterColumnName = "EnterDetailColumn";
    private const string DeleteColumnName = "DeleteDetailColumn";

    private readonly DataStore _store;
    private readonly Offer _offer;
    private readonly InstallationGroup _group;
    private readonly ComboBox _detailCombo = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _detailsGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };
    private readonly BindingSource _detailsBindingSource = new();
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
        _detailsGrid.Columns.Add(new DataGridViewButtonColumn { Name = EnterColumnName, HeaderText = "Gir", Text = "İş Detayına Gir", UseColumnTextForButtonValue = true, Width = 130 });
        _detailsGrid.Columns.Add(new DataGridViewButtonColumn { Name = DeleteColumnName, HeaderText = "Sil", Text = "İş Detayını Sil", UseColumnTextForButtonValue = true, Width = 130 });

        _detailsGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (_detailsGrid.Rows[e.RowIndex].DataBoundItem is not WorkDetail detail) return;
            var clickedColumn = _detailsGrid.Columns[e.ColumnIndex].Name;

            if (clickedColumn == EnterColumnName)
            {
                using var form = new MaterialListForm(_store, _offer, _group, detail);
                form.ShowDialog();
                _offer.LastUpdated = DateTime.Now;
                _store.MarkDirty();
                RefreshData();
            }
            else if (clickedColumn == DeleteColumnName)
            {
                var confirm = MessageBox.Show("Bu iş detayını silmek istediğinize emin misiniz?", "İş Detayı Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                _group.WorkDetails.Remove(detail);
                _store.MarkDirty();
                RefreshData();
            }
        };


        _detailsGrid.EnableHeadersVisualStyles = false;
        _detailsGrid.ColumnHeadersDefaultCellStyle.Font = new Font(_detailsGrid.Font, FontStyle.Bold);
        _detailsGrid.DataSource = _detailsBindingSource;
    }

    private void AddDetail()
    {
        if (_detailCombo.SelectedItem is not string name || string.IsNullOrWhiteSpace(name)) return;

        var exists = _group.WorkDetails.Any(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            MessageBox.Show("Aynı isimde iş detayı bu listede zaten mevcut.");
            return;
        }

        _group.WorkDetails.Add(new WorkDetail { Name = name.Trim() });
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

        if (_detailCombo.Items.Count > 0 && _detailCombo.SelectedIndex < 0)
        {
            _detailCombo.SelectedIndex = 0;
        }

        if (!ReferenceEquals(_detailsBindingSource.DataSource, _group.WorkDetails))
        {
            _detailsBindingSource.DataSource = _group.WorkDetails;
        }

        _detailsBindingSource.ResetBindings(false);

        _materialTotalLabel.Text = $"Malzemelerin Toplamı: {_group.WorkDetails.Sum(x => x.MaterialTotal):N2}";
        _laborTotalLabel.Text = $"İşçiliklerin Toplamı: {_group.WorkDetails.Sum(x => x.LaborTotal):N2}";
        _generalTotalLabel.Text = $"Genel Toplam: {_group.TotalAmount:N2}";
    }
}
