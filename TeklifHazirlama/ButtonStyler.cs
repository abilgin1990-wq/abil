namespace TeklifHazirlama;

public static class ButtonStyler
{
    private static readonly Color PrimaryBlue = Color.FromArgb(70, 130, 200);

    public static void Apply(Control root)
    {
        foreach (Control control in root.Controls)
        {
            if (control is Button button)
            {
                button.UseVisualStyleBackColor = false;
                button.BackColor = PrimaryBlue;
                button.ForeColor = Color.White;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.FromArgb(45, 100, 170);
                button.FlatAppearance.BorderSize = 1;
            }

            if (control.HasChildren)
            {
                Apply(control);
            }
        }
    }
}
