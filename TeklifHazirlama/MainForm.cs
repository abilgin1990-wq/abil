namespace TeklifHazirlama;

public class MainForm : Form
{
    private const string DetailColumnName = "DetailColumn";
    private const string DeleteColumnName = "DeleteColumn";

    private readonly DataStore _store = new();
    private readonly TextBox _companyText = new() { Width = 220 };
    private readonly TextBox _projectText = new() { Width = 220 };
    private readonly DataGridView _offersGrid = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, ReadOnly = true };
    private readonly BindingSource _offersBindingSource = new();

    public MainForm()
    {
        Text = "Teklif Hazırlama Programı";
        Width = 1200;
        Height = 700;

        Controls.Add(BuildLayout());
        BuildMenu();
        ConfigureOfferGrid();
        RefreshOfferGrid();

        ButtonStyler.Apply(this);

        FormClosing += (_, e) =>
        {
            if (!PromptToSaveChanges())
            {
                e.Cancel = true;
            }
        };
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var addButton = new Button { Text = "Ekle", Width = 120, Height = 32, Margin = new Padding(20, 2, 0, 0) };
        addButton.Click += (_, _) => AddOffer();

        var header = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        header.Controls.AddRange([
            new Label { Text = "Firma Adı", AutoSize = true, Padding = new Padding(0, 8, 0, 0) }, _companyText,
            new Label { Text = "Proje Adı", AutoSize = true, Padding = new Padding(20, 8, 0, 0) }, _projectText,
            addButton
        ]);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(_offersGrid, 0, 1);

        return root;
    }

    private void BuildMenu()
    {
        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("Dosya");
        var settings = new ToolStripMenuItem("Ayarlar");
        var load = new ToolStripMenuItem("Yükle");
        var save = new ToolStripMenuItem("Kaydet");
        var saveAs = new ToolStripMenuItem("Farklı Kaydet");
        var close = new ToolStripMenuItem("Kapat");

        settings.Click += (_, _) => OpenSettings();
        load.Click += (_, _) => LoadFromFile();
        save.Click += (_, _) => Save();
        saveAs.Click += (_, _) => SaveAs();
        close.Click += (_, _) => Close();

        file.DropDownItems.AddRange([settings, load, save, saveAs, close]);
        menu.Items.Add(file);

        MainMenuStrip = menu;
        Controls.Add(menu);
    }

    private void ConfigureOfferGrid()
    {
        _offersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Firma", DataPropertyName = nameof(Offer.CompanyName), Width = 200 });
        _offersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Proje", DataPropertyName = nameof(Offer.ProjectName), Width = 200 });
        _offersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Toplam Tutar", DataPropertyName = nameof(Offer.TotalWithVat), Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
        _offersGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Son Güncelleme", DataPropertyName = nameof(Offer.LastUpdated), Width = 170, DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } });
        _offersGrid.Columns.Add(new DataGridViewButtonColumn { Name = DetailColumnName, HeaderText = "Detay", Text = "Teklif Detayını Gör", UseColumnTextForButtonValue = true, Width = 140, FlatStyle = FlatStyle.Flat, DefaultCellStyle = new DataGridViewCellStyle { BackColor = ButtonStyler.PrimaryBlue, ForeColor = Color.White, SelectionBackColor = ButtonStyler.PrimaryBlue, SelectionForeColor = Color.White } });
        _offersGrid.Columns.Add(new DataGridViewButtonColumn { Name = DeleteColumnName, HeaderText = "Sil", Text = "Teklifi Sil", UseColumnTextForButtonValue = true, Width = 120, FlatStyle = FlatStyle.Flat, DefaultCellStyle = new DataGridViewCellStyle { BackColor = ButtonStyler.PrimaryBlue, ForeColor = Color.White, SelectionBackColor = ButtonStyler.PrimaryBlue, SelectionForeColor = Color.White } });

        _offersGrid.CellContentClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (_offersGrid.Rows[e.RowIndex].DataBoundItem is not Offer offer) return;

            var clickedColumn = _offersGrid.Columns[e.ColumnIndex].Name;
            if (clickedColumn == DetailColumnName)
            {
                using var detailForm = new OfferDetailForm(_store, offer)
                {
                    StartPosition = FormStartPosition.Manual,
                    Size = Size,
                    Location = Location
                };

                Hide();
                detailForm.ShowDialog();
                Show();

                offer.LastUpdated = DateTime.Now;
                _store.MarkDirty();
                RefreshOfferGrid();
            }
            else if (clickedColumn == DeleteColumnName)
            {
                var confirm = MessageBox.Show("Bu teklifi silmek istediğinize emin misiniz?", "Teklif Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                _store.State.Offers.Remove(offer);
                _store.MarkDirty();
                RefreshOfferGrid();

            }
        };


        _offersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _offersGrid.EnableHeadersVisualStyles = false;
        _offersGrid.ColumnHeadersDefaultCellStyle.Font = new Font(_offersGrid.Font, FontStyle.Bold);
        _offersGrid.DataSource = _offersBindingSource;
    }

    private void AddOffer()
    {
        var companyName = _companyText.Text.Trim();
        var projectName = _projectText.Text.Trim();
        if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(projectName))
        {
            MessageBox.Show("Firma ve proje alanları zorunludur.");
            return;
        }

        var isDuplicate = _store.State.Offers.Any(o =>
            string.Equals(o.CompanyName, companyName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(o.ProjectName, projectName, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            MessageBox.Show("Aynı firma adı ve proje ismi ile teklif zaten mevcut.");
            return;
        }

        _store.State.Offers.Add(new Offer
        {
            CompanyName = companyName,
            ProjectName = projectName,
            LastUpdated = DateTime.Now
        });

        _companyText.Clear();
        _projectText.Clear();
        _store.MarkDirty();
        RefreshOfferGrid();

    }

    private void RefreshOfferGrid()
    {
        if (!ReferenceEquals(_offersBindingSource.DataSource, _store.State.Offers))
        {
            _offersBindingSource.DataSource = _store.State.Offers;
        }

        _offersBindingSource.ResetBindings(false);
    }

    private void OpenSettings()
    {
        using var settingsForm = new SettingsForm(_store);
        settingsForm.ShowDialog();
        RefreshOfferGrid();

    }

    private void LoadFromFile()
    {
        if (!PromptToSaveChanges()) return;

        using var dialog = new OpenFileDialog { Filter = "Teklif Dosyası (*.json)|*.json", Title = "Yükle" };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            _store.Load(dialog.FileName);
            RefreshOfferGrid();

        }
        catch (Exception ex)
        {
            MessageBox.Show($"Yükleme sırasında hata: {ex.Message}");
        }
    }

    private void Save()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_store.CurrentPath))
            {
                SaveAs();
                return;
            }

            _store.Save();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kayıt sırasında hata: {ex.Message}");
        }
    }

    private void SaveAs()
    {
        using var dialog = new SaveFileDialog { Filter = "Teklif Dosyası (*.json)|*.json", Title = "Farklı Kaydet" };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            _store.SaveAs(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Kayıt sırasında hata: {ex.Message}");
        }
    }

    private bool PromptToSaveChanges()
    {
        if (!_store.IsDirty) return true;

        var result = MessageBox.Show("Değişiklikleri kaydetmek ister misiniz?", "Kapat", MessageBoxButtons.YesNoCancel);
        if (result == DialogResult.Cancel) return false;
        if (result == DialogResult.Yes)
        {
            Save();
            return !_store.IsDirty;
        }

        return true;
    }
}
