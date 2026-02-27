using TeklifHazirlama.Models;
using TeklifHazirlama.Services;

namespace TeklifHazirlama.Forms;

public sealed class PlumbingGroupForm : Form
{
    private readonly PlumbingGroupQuote _group;
    private readonly InMemoryDataStore _store = InMemoryDataStore.Instance;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
    private readonly TextBox _txtNewJobDetail = new() { PlaceholderText = "Yeni İş Detayı" };

    public PlumbingGroupForm(PlumbingGroupQuote group)
    {
        _group = group;
        Text = $"Tesisat Grubu - {_group.Name}";
        Width = 1000;
        Height = 620;

        var btnAdd = new Button { Text = "İş Detayı Ekle" };
        btnAdd.Click += (_, _) => AddJobDetail();

        var btnOpen = new Button { Text = "Seçili İş Detayına Gir" };
        btnOpen.Click += (_, _) => OpenJobDetail();

        var btnDelete = new Button { Text = "Sil" };
        btnDelete.Click += (_, _) => DeleteJobDetail();

        var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45 };
        topPanel.Controls.AddRange([_txtNewJobDetail, btnAdd, btnOpen, btnDelete]);

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(JobDetailQuote.Name), HeaderText = "İş Detayı", Width = 280 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(JobDetailQuote.TotalAmount), HeaderText = "Toplam Tutar", Width = 150 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DetailText", HeaderText = "Detay", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        Controls.Add(_grid);
        Controls.Add(topPanel);

        RefreshGrid();
    }

    private void AddJobDetail()
    {
        var name = _txtNewJobDetail.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("İş detayı boş olamaz.");
            return;
        }

        if (_group.JobDetails.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Bu iş detayı zaten var.");
            return;
        }

        _store.AddJobDetailGroup(name);
        _group.JobDetails.Add(new JobDetailQuote { Name = name });
        _txtNewJobDetail.Clear();
        RefreshGrid();
    }

    private void DeleteJobDetail()
    {
        if (GetSelectedJobDetail() is not { } selected)
        {
            MessageBox.Show("Lütfen silmek için bir iş detayı seçin.");
            return;
        }

        var answer = MessageBox.Show("Silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer == DialogResult.Yes)
        {
            _group.JobDetails.Remove(selected);
            RefreshGrid();
        }
    }

    private void OpenJobDetail()
    {
        if (GetSelectedJobDetail() is not { } selected)
        {
            MessageBox.Show("Lütfen açmak için bir iş detayı seçin.");
            return;
        }

        using var form = new JobDetailForm(_group.Name, selected);
        form.ShowDialog(this);
        RefreshGrid();
    }

    private JobDetailQuote? GetSelectedJobDetail()
    {
        if (_grid.CurrentRow?.DataBoundItem is JobDetailRow row)
        {
            return row.Source;
        }

        return null;
    }

    private void RefreshGrid()
    {
        _grid.DataSource = _group.JobDetails
            .OrderBy(x => x.Name)
            .Select(x => new JobDetailRow
            {
                Source = x,
                Name = x.Name,
                TotalAmount = x.TotalAmount,
                DetailText = $"Malzeme Satırı: {x.MaterialLines.Count}, İşçilik: {x.LaborTotal:n2} ₺"
            })
            .ToList();
    }

    private sealed class JobDetailRow
    {
        public required JobDetailQuote Source { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
        public string DetailText { get; init; } = string.Empty;
    }
}
