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

    public EditSiteForm(ConstructionSite site)
    {
        _site = site;

        Text = "Şantiye Düzenle";
        Width = 520;
        Height = 370;
        StartPosition = FormStartPosition.CenterParent;

        InitializeLayout();
        FillForm();
    }

    private void InitializeLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 6
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddLabeledTextBox(layout, "Şantiye İsmi", _siteNameTextBox, 0);
        AddLabeledTextBox(layout, "Şantiye Şefi", _managerNameTextBox, 1);
        AddLabeledTextBox(layout, "Şefi Telefon", _managerPhoneTextBox, 2);
        AddLabeledTextBox(layout, "Usta İsmi", _workerNameTextBox, 3);
        AddLabeledTextBox(layout, "Usta Telefon", _workerPhoneTextBox, 4);

        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill
        };

        var saveButton = new Button { Text = "Kaydet", AutoSize = true };
        saveButton.Click += SaveButtonClick;

        var cancelButton = new Button { Text = "İptal", AutoSize = true };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        layout.Controls.Add(buttonPanel, 0, 5);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);
    }

    private static void AddLabeledTextBox(TableLayoutPanel layout, string label, TextBox textBox, int row)
    {
        layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        textBox.Dock = DockStyle.Fill;
        layout.Controls.Add(textBox, 1, row);
    }

    private void FillForm()
    {
        _siteNameTextBox.Text = _site.Name;
        _managerNameTextBox.Text = _site.SiteManagerName;
        _managerPhoneTextBox.Text = _site.SiteManagerPhone;
        _workerNameTextBox.Text = _site.WorkerName;
        _workerPhoneTextBox.Text = _site.WorkerPhone;
    }

    private void SaveButtonClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_siteNameTextBox.Text))
        {
            MessageBox.Show("Şantiye adı boş olamaz.");
            return;
        }

        _site.Name = _siteNameTextBox.Text.Trim();
        _site.SiteManagerName = _managerNameTextBox.Text.Trim();
        _site.SiteManagerPhone = _managerPhoneTextBox.Text.Trim();
        _site.WorkerName = _workerNameTextBox.Text.Trim();
        _site.WorkerPhone = _workerPhoneTextBox.Text.Trim();
        _site.LastUpdated = DateTime.Now;

        DialogResult = DialogResult.OK;
    }
}
