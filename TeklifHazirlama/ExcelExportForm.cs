namespace TeklifHazirlama;

public class ExcelExportForm : Form
{
    private readonly DataStore _store;
    private readonly ComboBox _pageCombo = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _offerCombo = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _groupCombo = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _detailCombo = new() { Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private bool _isRefreshingSelectors;

    public ExcelExportForm(DataStore store)
    {
        _store = store;

        Text = "Excele Aktar";
        Width = 620;
        Height = 300;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(BuildLayout());
        BindData();

        ButtonStyler.Apply(this);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(12)
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(root, 0, "Sayfa", _pageCombo);
        AddRow(root, 1, "Teklif", _offerCombo);
        AddRow(root, 2, "Tesisat Grubu", _groupCombo);
        AddRow(root, 3, "İş Detayı", _detailCombo);

        var exportButton = new Button { Text = "Excele Aktar", Width = 140 };
        exportButton.Click += (_, _) => Export();

        root.Controls.Add(new Label { Text = string.Empty, AutoSize = true }, 0, 4);
        root.Controls.Add(exportButton, 1, 4);

        _pageCombo.SelectedIndexChanged += (_, _) =>
        {
            if (_isRefreshingSelectors) return;
            RefreshSelectors();
        };
        _offerCombo.SelectedIndexChanged += (_, _) =>
        {
            if (_isRefreshingSelectors) return;
            RefreshSelectors();
        };
        _groupCombo.SelectedIndexChanged += (_, _) =>
        {
            if (_isRefreshingSelectors) return;
            RefreshSelectors();
        };

        return root;
    }

    private static void AddRow(TableLayoutPanel root, int row, string label, Control control)
    {
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 8, 8, 0) }, 0, row);
        root.Controls.Add(control, 1, row);
    }

    private void BindData()
    {
        _pageCombo.Items.Clear();
        _pageCombo.Items.AddRange(new object[]
        {
            "Teklif Ana Kalemleri Tablosu",
            "Tesisat Grubundaki İş Detayı Listesi",
            "İş Detayı İçindeki Malzeme Listesi"
        });

        if (_pageCombo.Items.Count > 0)
        {
            _pageCombo.SelectedIndex = 0;
        }

        RefreshSelectors();
    }

    private void RefreshSelectors()
    {
        _isRefreshingSelectors = true;
        try
        {
            var previousOfferId = (_offerCombo.SelectedItem as ComboItem<Offer>)?.Value.Id;
            var previousGroupId = (_groupCombo.SelectedItem as ComboItem<InstallationGroup>)?.Value.Id;
            var previousDetailId = (_detailCombo.SelectedItem as ComboItem<WorkDetail>)?.Value.Id;

            _offerCombo.Items.Clear();
            foreach (var offer in _store.State.Offers)
            {
                _offerCombo.Items.Add(new ComboItem<Offer>($"{offer.CompanyName} - {offer.ProjectName}", offer));
            }
            SelectOfferById(previousOfferId);

            _groupCombo.Items.Clear();
            var selectedOffer = (_offerCombo.SelectedItem as ComboItem<Offer>)?.Value;
            if (selectedOffer != null)
            {
                foreach (var group in selectedOffer.InstallationGroups)
                {
                    _groupCombo.Items.Add(new ComboItem<InstallationGroup>(group.Name, group));
                }
            }
            SelectGroupById(previousGroupId);

            _detailCombo.Items.Clear();
            var selectedGroup = (_groupCombo.SelectedItem as ComboItem<InstallationGroup>)?.Value;
            if (selectedGroup != null)
            {
                foreach (var detail in selectedGroup.WorkDetails)
                {
                    _detailCombo.Items.Add(new ComboItem<WorkDetail>(detail.Name, detail));
                }
            }
            SelectDetailById(previousDetailId);

            var pageIndex = _pageCombo.SelectedIndex;
            _offerCombo.Enabled = pageIndex >= 0;
            _groupCombo.Enabled = pageIndex is 1 or 2;
            _detailCombo.Enabled = pageIndex == 2;
        }
        finally
        {
            _isRefreshingSelectors = false;
        }
    }

    private void SelectOfferById(Guid? id)
    {
        if (_offerCombo.Items.Count == 0) return;

        if (id != null)
        {
            for (var i = 0; i < _offerCombo.Items.Count; i++)
            {
                if (_offerCombo.Items[i] is ComboItem<Offer> item && item.Value.Id == id)
                {
                    _offerCombo.SelectedIndex = i;
                    return;
                }
            }
        }

        _offerCombo.SelectedIndex = 0;
    }

    private void SelectGroupById(Guid? id)
    {
        if (_groupCombo.Items.Count == 0) return;

        if (id != null)
        {
            for (var i = 0; i < _groupCombo.Items.Count; i++)
            {
                if (_groupCombo.Items[i] is ComboItem<InstallationGroup> item && item.Value.Id == id)
                {
                    _groupCombo.SelectedIndex = i;
                    return;
                }
            }
        }

        _groupCombo.SelectedIndex = 0;
    }

    private void SelectDetailById(Guid? id)
    {
        if (_detailCombo.Items.Count == 0) return;

        if (id != null)
        {
            for (var i = 0; i < _detailCombo.Items.Count; i++)
            {
                if (_detailCombo.Items[i] is ComboItem<WorkDetail> item && item.Value.Id == id)
                {
                    _detailCombo.SelectedIndex = i;
                    return;
                }
            }
        }

        _detailCombo.SelectedIndex = 0;
    }

    private void Export()
    {
        var rows = BuildRows();
        if (rows.Count == 0)
        {
            MessageBox.Show("Aktarılacak veri bulunamadı.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Excel CSV (*.csv)|*.csv",
            Title = "Excele Aktar",
            FileName = "teklif_export.csv"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var csv = string.Join(Environment.NewLine, rows.Select(row => string.Join(";", row.Select(EscapeCsv))));
        File.WriteAllText(dialog.FileName, "\uFEFF" + csv);
        MessageBox.Show("Excel aktarımı tamamlandı.");
    }

    private List<string[]> BuildRows()
    {
        var pageIndex = _pageCombo.SelectedIndex;
        var offer = (_offerCombo.SelectedItem as ComboItem<Offer>)?.Value;
        var group = (_groupCombo.SelectedItem as ComboItem<InstallationGroup>)?.Value;
        var detail = (_detailCombo.SelectedItem as ComboItem<WorkDetail>)?.Value;

        if (offer == null) return new List<string[]>();

        if (pageIndex == 0)
        {
            var rows = new List<string[]> { new[] { "Tesisat Grubu", "Tutar" } };
            rows.AddRange(offer.InstallationGroups.Select(g => new[] { g.Name, g.TotalAmount.ToString("N2") }));
            return rows;
        }

        if (group == null) return new List<string[]>();

        if (pageIndex == 1)
        {
            var rows = new List<string[]> { new[] { "İş Detayı", "Tutar" } };
            rows.AddRange(group.WorkDetails.Select(d => new[] { d.Name, d.GrandTotal.ToString("N2") }));
            return rows;
        }

        if (detail == null) return new List<string[]>();

        var materialRows = new List<string[]>
        {
            new[] { "Malzeme", "Marka", "Adet", "Liste Fiyatı", "PB", "İskonto %", "Birim Fiyat", "İşçilik Birim", "Malzeme Toplam", "İşçilik Toplam", "Genel Toplam" }
        };

        materialRows.AddRange(detail.Materials.Select(m => new[]
        {
            m.MaterialName,
            m.Brand,
            m.Quantity.ToString("N2"),
            m.OriginalListPrice.ToString("N2"),
            m.Currency,
            m.DiscountPercent.ToString("N2"),
            m.UnitPrice.ToString("N2"),
            m.LaborUnitPrice.ToString("N2"),
            m.MaterialTotal.ToString("N2"),
            m.LaborTotal.ToString("N2"),
            m.GrandTotal.ToString("N2")
        }));

        return materialRows;
    }

    private static string EscapeCsv(string input)
    {
        if (input.Contains(';') || input.Contains('"') || input.Contains('\n'))
        {
            return '"' + input.Replace("\"", "\"\"") + '"';
        }

        return input;
    }

    private sealed class ComboItem<T>(string text, T value)
    {
        public string Text { get; } = text;
        public T Value { get; } = value;
        public override string ToString() => Text;
    }
}
