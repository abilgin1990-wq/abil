namespace TeklifHazirlama;

public static class FileMenuHelper
{
    public static void Attach(Form form, DataStore store, Action? onSettingsUpdated = null, Action? onLoaded = null, bool allowLoad = false)
    {
        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("Dosya");
        var settings = new ToolStripMenuItem("Ayarlar");
        var load = new ToolStripMenuItem("Yükle") { Enabled = allowLoad };
        var save = new ToolStripMenuItem("Kaydet");
        var saveAs = new ToolStripMenuItem("Farklı Kaydet");
        var close = new ToolStripMenuItem("Kapat");

        settings.Click += (_, _) =>
        {
            using var settingsForm = new SettingsForm(store);
            settingsForm.ShowDialog(form);
            onSettingsUpdated?.Invoke();
        };

        load.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog { Filter = "Teklif Dosyası (*.json)|*.json", Title = "Yükle" };
            if (dialog.ShowDialog(form) != DialogResult.OK) return;

            try
            {
                store.Load(dialog.FileName);
                onLoaded?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yükleme sırasında hata: {ex.Message}");
            }
        };

        save.Click += (_, _) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(store.CurrentPath))
                {
                    SaveAs(form, store);
                    return;
                }

                store.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında hata: {ex.Message}");
            }
        };

        saveAs.Click += (_, _) => SaveAs(form, store);
        close.Click += (_, _) => form.Close();

        file.DropDownItems.AddRange([settings, load, save, saveAs, close]);
        menu.Items.Add(file);

        form.MainMenuStrip = menu;
        form.Controls.Add(menu);
        menu.BringToFront();

        var menuHeight = menu.Height > 0 ? menu.Height : 28;
        if (form.Padding.Top < menuHeight)
        {
            form.Padding = new Padding(form.Padding.Left, menuHeight, form.Padding.Right, form.Padding.Bottom);
        }
    }

    private static void SaveAs(Form form, DataStore store)
    {
        using var dialog = new SaveFileDialog { Filter = "Teklif Dosyası (*.json)|*.json", Title = "Farklı Kaydet" };
        if (dialog.ShowDialog(form) != DialogResult.OK) return;

        try
        {
            store.SaveAs(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kayıt sırasında hata: {ex.Message}");
        }
    }
}
