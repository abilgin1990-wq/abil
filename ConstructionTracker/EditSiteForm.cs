using System.Windows.Forms;

namespace ConstructionTracker;

public class EditSiteForm : Form
{
    private readonly ConstructionSite _site;
    private readonly TextBox _siteNameTextBox = new();
    private readonly TextBox _managerNameTextBox = new();
    private readonly TextBox _managerPhoneTextBox = new();
    private readonly TextBox _workerNameTextBox = new();
    private readonly TextBox _workerPhoneTextBox = new();
    private readonly DateTimePicker _startDatePicker = new();

    public EditSiteForm(ConstructionSite site)
    {
        _site = site;
        Text = "Şantiye Düzenle";
        Width = 520;
        Height = 420;
        StartPosition = FormStartPosition.CenterParent;
        InitializeLayout();
        FillForm();
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = 7 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Add(layout, "Şantiye İsmi", _siteNameTextBox, 0);
        Add(layout, "Şantiye Şefi", _managerNameTextBox, 1);
        Add(layout, "Şantiye Şefi Telefon", _managerPhoneTextBox, 2);
        Add(layout, "Usta İsmi", _workerNameTextBox, 3);
        Add(layout, "Usta Telefon", _workerPhoneTextBox, 4);
        _startDatePicker.Format = DateTimePickerFormat.Short;
        Add(layout, "Başlangıç Tarihi", _startDatePicker, 5);

        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        var save = new Button { Text = "Kaydet", AutoSize = true };
        save.Click += SaveButtonClick;
        var cancel = new Button { Text = "İptal", AutoSize = true };
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);

        layout.Controls.Add(buttons, 0, 6);
        layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout);
    }

    private static void Add(TableLayoutPanel l, string label, Control c, int row)
    {
        l.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        c.Dock = DockStyle.Fill;
        l.Controls.Add(c, 1, row);
    }

    private void FillForm()
    {
        _siteNameTextBox.Text = _site.Name;
        _managerNameTextBox.Text = _site.SiteManagerName;
        _managerPhoneTextBox.Text = _site.SiteManagerPhone;
        _workerNameTextBox.Text = _site.WorkerName;
        _workerPhoneTextBox.Text = _site.WorkerPhone;
        _startDatePicker.Value = _site.StartDate;
    }

    private void SaveButtonClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_siteNameTextBox.Text)) { MessageBox.Show("Şantiye adı boş olamaz."); return; }
        _site.Name = _siteNameTextBox.Text.Trim();
        _site.SiteManagerName = _managerNameTextBox.Text.Trim();
        _site.SiteManagerPhone = _managerPhoneTextBox.Text.Trim();
        _site.WorkerName = _workerNameTextBox.Text.Trim();
        _site.WorkerPhone = _workerPhoneTextBox.Text.Trim();
        _site.StartDate = _startDatePicker.Value.Date;
        _site.LastUpdated = DateTime.Now;
        DialogResult = DialogResult.OK;
    }
}
