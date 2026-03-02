namespace AbilSantiyeTakip;

public static class Prompt
{
    public static string? Show(string title, string message, string initial = "")
    {
        using var form = new Form { Text = title, Width = 400, Height = 170, StartPosition = FormStartPosition.CenterParent };
        var lbl = new Label { Left = 15, Top = 15, Width = 350, Text = message };
        var txt = new TextBox { Left = 15, Top = 40, Width = 350, Text = initial };
        var ok = new Button { Text = "Tamam", Left = 210, Width = 75, Top = 75, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "İptal", Left = 290, Width = 75, Top = 75, DialogResult = DialogResult.Cancel };
        form.Controls.AddRange([lbl, txt, ok, cancel]);
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        return form.ShowDialog() == DialogResult.OK ? txt.Text.Trim() : null;
    }
}
