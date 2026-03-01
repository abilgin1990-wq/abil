using System.ComponentModel;
using System.Windows.Forms;

namespace ConstructionTracker;

public class MainForm : Form
{
    private readonly BindingList<ConstructionSite> _sites = new();

    private readonly TextBox _siteNameTextBox = new();
    private readonly TextBox _managerNameTextBox = new();
    private readonly TextBox _managerPhoneTextBox = new();
    private readonly TextBox _workerNameTextBox = new();
    private readonly TextBox _workerPhoneTextBox = new();
    private readonly Button _addButton = new();

    private readonly DataGridView _siteGrid = new();

    public MainForm()
    {
        Text = "Şantiye Yönetimi";
        Width = 1080;
        Height = 720;
        StartPosition = FormStartPosition.CenterScreen;

        InitializeLayout();
        ConfigureGrid();
    }

    private void InitializeLayout()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(12)
        };

        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var formPanel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 4,
            Dock = DockStyle.Top
        };

        for (var i = 0; i < 4; i++)
        {
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }

        AddLabeledControl(formPanel, "Şantiye İsmi", _siteNameTextBox, 0, 0);
        AddLabeledControl(formPanel, "Şantiye Şefi", _managerNameTextBox, 1, 0);
        AddLabeledControl(formPanel, "Şantiye Şefi Telefon", _managerPhoneTextBox, 2, 0);
        AddLabeledControl(formPanel, "Usta İsmi", _workerNameTextBox, 0, 2);
        AddLabeledControl(formPanel, "Usta Telefon", _workerPhoneTextBox, 1, 2);

        _addButton.Text = "Ekle";
        _addButton.AutoSize = true;
        _addButton.Click += AddSite;
        formPanel.Controls.Add(_addButton, 2, 3);

        _siteGrid.Dock = DockStyle.Fill;

        mainLayout.Controls.Add(formPanel, 0, 0);
        mainLayout.Controls.Add(_siteGrid, 0, 1);

        Controls.Add(mainLayout);
    }

    private static void AddLabeledControl(TableLayoutPanel panel, string label, Control control, int column, int row)
    {
        var lbl = new Label
        {
            Text = label,
            AutoSize = true,
            Margin = new Padding(6, 8, 6, 4)
        };
        control.Margin = new Padding(6, 0, 6, 8);
        control.Width = 220;

        panel.Controls.Add(lbl, column, row);
        panel.Controls.Add(control, column, row + 1);
    }

    private void ConfigureGrid()
    {
        _siteGrid.AutoGenerateColumns = false;
        _siteGrid.AllowUserToAddRows = false;
        _siteGrid.ReadOnly = true;

        _siteGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Şantiye İsmi",
            DataPropertyName = nameof(ConstructionSite.Name),
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });

        _siteGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Son Güncelleme",
            DataPropertyName = nameof(ConstructionSite.LastUpdated),
            Width = 180,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" }
        });

        _siteGrid.Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = "Detay",
            Text = "Şantiye Detayını Gör",
            UseColumnTextForButtonValue = true,
            Width = 170,
            Name = "DetailButton"
        });

        _siteGrid.Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = "Düzenle",
            Text = "Düzenle",
            UseColumnTextForButtonValue = true,
            Width = 120,
            Name = "EditButton"
        });

        _siteGrid.CellContentClick += SiteGridCellContentClick;
        _siteGrid.DataSource = _sites;
    }

    private void AddSite(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_siteNameTextBox.Text))
        {
            MessageBox.Show("Şantiye ismi zorunludur.");
            return;
        }

        var site = new ConstructionSite
        {
            Name = _siteNameTextBox.Text.Trim(),
            SiteManagerName = _managerNameTextBox.Text.Trim(),
            SiteManagerPhone = _managerPhoneTextBox.Text.Trim(),
            WorkerName = _workerNameTextBox.Text.Trim(),
            WorkerPhone = _workerPhoneTextBox.Text.Trim(),
            LastUpdated = DateTime.Now
        };

        _sites.Add(site);
        ClearInputs();
    }

    private void ClearInputs()
    {
        _siteNameTextBox.Clear();
        _managerNameTextBox.Clear();
        _managerPhoneTextBox.Clear();
        _workerNameTextBox.Clear();
        _workerPhoneTextBox.Clear();
    }

    private void SiteGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        var site = _sites[e.RowIndex];
        var columnName = _siteGrid.Columns[e.ColumnIndex].Name;

        if (columnName == "DetailButton")
        {
            using var detailForm = new SiteTrackingForm(site, Size);
            Hide();
            detailForm.ShowDialog(this);
            Show();
        }
        else if (columnName == "EditButton")
        {
            using var editForm = new EditSiteForm(site);
            if (editForm.ShowDialog(this) == DialogResult.OK)
            {
                site.LastUpdated = DateTime.Now;
                _siteGrid.Refresh();
            }
        }
    }
}
