namespace AbilSantiyeTakip;

public class SiteEditForm : Form
{
    public SiteEditForm(ConstructionSite site)
    {
        Text = "Şantiye Düzenle";
        Width = 500;
        Height = 340;
        StartPosition = FormStartPosition.CenterParent;

        var txtSite = new TextBox { Left = 170, Top = 20, Width = 280, Text = site.SiteName };
        var txtChief = new TextBox { Left = 170, Top = 60, Width = 280, Text = site.ChiefName };
        var txtChiefPhone = new TextBox { Left = 170, Top = 100, Width = 280, Text = site.ChiefPhone };
        var txtWorker = new TextBox { Left = 170, Top = 140, Width = 280, Text = site.WorkerName };
        var txtWorkerPhone = new TextBox { Left = 170, Top = 180, Width = 280, Text = site.WorkerPhone };

        Controls.AddRange([
            new Label { Left = 20, Top = 25, Text = "Şantiye İsmi", Width = 140 }, txtSite,
            new Label { Left = 20, Top = 65, Text = "Şantiye Şefi", Width = 140 }, txtChief,
            new Label { Left = 20, Top = 105, Text = "Şantiye Şefi Telefon", Width = 140 }, txtChiefPhone,
            new Label { Left = 20, Top = 145, Text = "Usta İsmi", Width = 140 }, txtWorker,
            new Label { Left = 20, Top = 185, Text = "Usta Telefon", Width = 140 }, txtWorkerPhone
        ]);

        var btnSave = new Button { Text = "Kaydet", Left = 280, Top = 235, Width = 80, DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "İptal", Left = 370, Top = 235, Width = 80, DialogResult = DialogResult.Cancel };
        btnSave.Click += (_, _) =>
        {
            site.SiteName = txtSite.Text.Trim();
            site.ChiefName = txtChief.Text.Trim();
            site.ChiefPhone = txtChiefPhone.Text.Trim();
            site.WorkerName = txtWorker.Text.Trim();
            site.WorkerPhone = txtWorkerPhone.Text.Trim();
        };
        Controls.AddRange([btnSave, btnCancel]);
    }
}
