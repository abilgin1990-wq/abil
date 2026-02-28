namespace TeklifHazirlama;

public class OfferEditForm : Form
{
    private readonly TextBox _companyText = new() { Width = 220 };
    private readonly TextBox _projectText = new() { Width = 220 };

    public string CompanyName => _companyText.Text.Trim();
    public string ProjectName => _projectText.Text.Trim();

    public OfferEditForm(string companyName, string projectName)
    {
        Text = "Teklif Düzenle";
        Width = 440;
        Height = 220;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        _companyText.Text = companyName;
        _projectText.Text = projectName;

        Controls.Add(BuildLayout());
        ButtonStyler.Apply(this);
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(12)
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.Controls.Add(new Label { Text = "Firma Adı", AutoSize = true, Padding = new Padding(0, 8, 8, 0) }, 0, 0);
        root.Controls.Add(_companyText, 1, 0);

        root.Controls.Add(new Label { Text = "Proje Adı", AutoSize = true, Padding = new Padding(0, 8, 8, 0) }, 0, 1);
        root.Controls.Add(_projectText, 1, 1);

        var buttonPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var saveButton = new Button { Text = "Kaydet", Width = 100 };
        var cancelButton = new Button { Text = "İptal", Width = 100 };

        saveButton.Click += (_, _) => SaveAndClose();
        cancelButton.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        root.Controls.Add(new Label { Text = string.Empty, AutoSize = true }, 0, 2);
        root.Controls.Add(buttonPanel, 1, 2);

        return root;
    }

    private void SaveAndClose()
    {
        if (string.IsNullOrWhiteSpace(CompanyName) || string.IsNullOrWhiteSpace(ProjectName))
        {
            MessageBox.Show("Firma ve proje alanları zorunludur.");
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
