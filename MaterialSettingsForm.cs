namespace AbilSantiyeTakip;

public class MaterialSettingsForm : Form
{
    private readonly List<string> _materials;
    private readonly TextBox _txt = new();
    private readonly DataGridView _grid = new();

    public MaterialSettingsForm(List<string> materials)
    {
        _materials = materials;
        Text = "Malzeme Ekleme Menüsü";
        Width = 600;
        Height = 450;
        StartPosition = FormStartPosition.CenterParent;

        Controls.Add(new Label { Text = "Menü Ekleme", Left = 10, Top = 15, Width = 120 });
        _txt.SetBounds(130, 12, 280, 25);
        var btnAdd = new Button { Text = "Ekle", Left = 420, Top = 10, Width = 80 };
        btnAdd.Click += (_, _) => AddMaterial();
        Controls.AddRange([_txt, btnAdd]);

        _grid.SetBounds(10, 50, 560, 310);
        _grid.AllowUserToAddRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.ReadOnly = true;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Malzeme İsmi", Width = 250 });
        _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Düzenle", Text = "Düzenle", UseColumnTextForButtonValue = true, Width = 120 });
        _grid.Columns.Add(new DataGridViewButtonColumn { HeaderText = "Sil", Text = "Sil", UseColumnTextForButtonValue = true, Width = 120 });
        _grid.CellContentClick += Grid_CellContentClick;
        Controls.Add(_grid);

        Controls.Add(new Button { Text = "Kapat", Left = 490, Top = 370, Width = 80, DialogResult = DialogResult.OK });
        RefreshGrid();
    }

    private void AddMaterial()
    {
        var name = _txt.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!_materials.Contains(name, StringComparer.OrdinalIgnoreCase)) _materials.Add(name);
        _txt.Clear();
        RefreshGrid();
    }

    private void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _materials.Count) return;
        if (e.ColumnIndex == 1)
        {
            var edited = Prompt.Show("Malzeme Düzenle", "Yeni isim:", _materials[e.RowIndex]);
            if (!string.IsNullOrWhiteSpace(edited)) _materials[e.RowIndex] = edited;
        }
        else if (e.ColumnIndex == 2)
        {
            _materials.RemoveAt(e.RowIndex);
        }
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        _grid.Rows.Clear();
        foreach (var material in _materials.OrderBy(x => x)) _grid.Rows.Add(material);
    }
}
