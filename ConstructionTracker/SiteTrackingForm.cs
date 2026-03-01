using System.ComponentModel;
using System.Windows.Forms;

namespace ConstructionTracker;

public class SiteTrackingForm : Form
{
    private readonly ConstructionSite _site;

    private readonly DateTimePicker _datePicker = new();
    private readonly TextBox _workerCountTextBox = new();
    private readonly ComboBox _workTypeCombo = new();
    private readonly DataGridView _attendanceGrid = new();
    private readonly Label _halfDaySummaryLabel = new();
    private readonly Label _fullDaySummaryLabel = new();

    public SiteTrackingForm(ConstructionSite site, Size parentSize)
    {
        _site = site;

        Text = "Şantiye Takip Sayfası";
        Width = parentSize.Width;
        Height = parentSize.Height;
        StartPosition = FormStartPosition.CenterParent;

        InitializeLayout();
        ConfigureAttendanceGrid();
        RefreshSummary();
    }

    private void InitializeLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };

        var attendanceTab = new TabPage("Şantiye Puantaj");
        var materialTab = new TabPage("Şantiyeye Giden Malzeme");

        attendanceTab.Controls.Add(BuildAttendanceTabContent());
        materialTab.Controls.Add(BuildMaterialTabContent());

        tabs.TabPages.Add(attendanceTab);
        tabs.TabPages.Add(materialTab);

        Controls.Add(tabs);
    }

    private Control BuildAttendanceTabContent()
    {
        var container = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 4
        };

        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = $"Şantiye: {_site.Name}",
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true
        };

        var inputPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            WrapContents = true
        };

        _datePicker.Format = DateTimePickerFormat.Short;
        _workerCountTextBox.Width = 120;
        _workTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _workTypeCombo.Width = 120;
        _workTypeCombo.DataSource = Enum.GetValues(typeof(WorkType));

        var addButton = new Button { Text = "Ekle", AutoSize = true };
        addButton.Click += AddAttendance;

        inputPanel.Controls.AddRange(new Control[]
        {
            new Label { Text = "Tarih", AutoSize = true, Padding = new Padding(0,8,0,0) },
            _datePicker,
            new Label { Text = "Çalışan Sayısı", AutoSize = true, Padding = new Padding(12,8,0,0) },
            _workerCountTextBox,
            new Label { Text = "Çalışma", AutoSize = true, Padding = new Padding(12,8,0,0) },
            _workTypeCombo,
            addButton
        });

        _attendanceGrid.Dock = DockStyle.Fill;

        var summaryPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.TopDown
        };

        summaryPanel.Controls.Add(_halfDaySummaryLabel);
        summaryPanel.Controls.Add(_fullDaySummaryLabel);

        container.Controls.Add(title, 0, 0);
        container.Controls.Add(inputPanel, 0, 1);
        container.Controls.Add(_attendanceGrid, 0, 2);
        container.Controls.Add(summaryPanel, 0, 3);

        return container;
    }

    private Control BuildMaterialTabContent()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };

        var title = new Label
        {
            Text = $"Şantiye: {_site.Name}",
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Top
        };

        var placeholder = new Label
        {
            Text = "Malzeme takibi için alan hazır.",
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 16, 0, 0)
        };

        panel.Controls.Add(placeholder);
        panel.Controls.Add(title);
        return panel;
    }

    private void ConfigureAttendanceGrid()
    {
        _attendanceGrid.AutoGenerateColumns = false;
        _attendanceGrid.AllowUserToAddRows = false;
        _attendanceGrid.ReadOnly = true;

        _attendanceGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Tarih",
            DataPropertyName = nameof(AttendanceRecord.Date),
            Width = 120,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
        });

        _attendanceGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Çalışan Sayısı",
            DataPropertyName = nameof(AttendanceRecord.WorkerCount),
            Width = 130
        });

        _attendanceGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Çalışma Detayı",
            DataPropertyName = nameof(AttendanceRecord.WorkType),
            Width = 140
        });

        _attendanceGrid.Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = "Düzenle",
            Text = "Düzenle",
            Name = "EditAttendanceButton",
            UseColumnTextForButtonValue = true,
            Width = 120
        });

        _attendanceGrid.Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = "Sil",
            Text = "Sil",
            Name = "DeleteAttendanceButton",
            UseColumnTextForButtonValue = true,
            Width = 100
        });

        _attendanceGrid.CellContentClick += AttendanceGridCellContentClick;
        _attendanceGrid.DataSource = _site.AttendanceRecords;
    }

    private void AddAttendance(object? sender, EventArgs e)
    {
        if (!int.TryParse(_workerCountTextBox.Text.Trim(), out var workerCount) || workerCount <= 0)
        {
            MessageBox.Show("Geçerli bir çalışan sayısı girin.");
            return;
        }

        var selectedDate = _datePicker.Value.Date;
        if (_site.AttendanceRecords.Any(x => x.Date.Date == selectedDate))
        {
            MessageBox.Show("Aynı güne birden fazla çalışma eklenemez.");
            return;
        }

        _site.AttendanceRecords.Add(new AttendanceRecord
        {
            Date = selectedDate,
            WorkerCount = workerCount,
            WorkType = (WorkType)_workTypeCombo.SelectedItem!
        });

        _site.LastUpdated = DateTime.Now;
        _workerCountTextBox.Clear();
        RefreshSummary();
    }

    private void AttendanceGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        var record = _site.AttendanceRecords[e.RowIndex];
        var columnName = _attendanceGrid.Columns[e.ColumnIndex].Name;

        if (columnName == "DeleteAttendanceButton")
        {
            _site.AttendanceRecords.Remove(record);
            _site.LastUpdated = DateTime.Now;
            RefreshSummary();
            return;
        }

        if (columnName == "EditAttendanceButton")
        {
            using var dialog = new EditAttendanceForm(record, _site.AttendanceRecords.Where((_, i) => i != e.RowIndex).ToList());
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _site.LastUpdated = DateTime.Now;
                _attendanceGrid.Refresh();
                RefreshSummary();
            }
        }
    }

    private void RefreshSummary()
    {
        var half = _site.AttendanceRecords.Where(x => x.WorkType == WorkType.YarimGun).ToList();
        var full = _site.AttendanceRecords.Where(x => x.WorkType == WorkType.TamGun).ToList();

        _halfDaySummaryLabel.Text = $"Yarım gün: {half.Count} gün, toplam çalışan: {half.Sum(x => x.WorkerCount)}";
        _fullDaySummaryLabel.Text = $"Tam gün: {full.Count} gün, toplam çalışan: {full.Sum(x => x.WorkerCount)}";
    }
}
