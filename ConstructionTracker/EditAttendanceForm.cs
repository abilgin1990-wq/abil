using System.Windows.Forms;

namespace ConstructionTracker;

public class EditAttendanceForm : Form
{
    private readonly AttendanceRecord _record;
    private readonly DateTimePicker _datePicker = new();
    private readonly TextBox _workerCountTextBox = new();
    private readonly ComboBox _workTypeCombo = new();

    public EditAttendanceForm(AttendanceRecord record)
    {
        _record = record;
        Text = "Puantaj Düzenle";
        Width = 420;
        Height = 240;
        StartPosition = FormStartPosition.CenterParent;
        InitializeLayout();
        FillData();
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label { Text = "Tarih", AutoSize = true }, 0, 0);
        _datePicker.Format = DateTimePickerFormat.Short;
        layout.Controls.Add(_datePicker, 1, 0);

        layout.Controls.Add(new Label { Text = "Çalışan Sayısı", AutoSize = true }, 0, 1);
        layout.Controls.Add(_workerCountTextBox, 1, 1);

        layout.Controls.Add(new Label { Text = "Çalışma Detayı", AutoSize = true }, 0, 2);
        _workTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _workTypeCombo.DataSource = Enum.GetValues(typeof(WorkType));
        layout.Controls.Add(_workTypeCombo, 1, 2);

        var btns = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        var save = new Button { Text = "Kaydet", AutoSize = true };
        save.Click += SaveClick;
        var cancel = new Button { Text = "İptal", AutoSize = true };
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        btns.Controls.Add(save); btns.Controls.Add(cancel);
        layout.Controls.Add(btns, 0, 3); layout.SetColumnSpan(btns, 2);
        Controls.Add(layout);
    }

    private void FillData()
    {
        _datePicker.Value = _record.Date;
        _workerCountTextBox.Text = _record.WorkerCount.ToString();
        _workTypeCombo.SelectedItem = _record.WorkType;
    }

    private void SaveClick(object? sender, EventArgs e)
    {
        if (!int.TryParse(_workerCountTextBox.Text.Trim(), out var wc) || wc <= 0) { MessageBox.Show("Geçerli bir çalışan sayısı girin."); return; }
        _record.Date = _datePicker.Value.Date;
        _record.WorkerCount = wc;
        _record.WorkType = (WorkType)_workTypeCombo.SelectedItem!;
        DialogResult = DialogResult.OK;
    }
}
