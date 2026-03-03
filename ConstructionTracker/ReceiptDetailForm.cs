using System.Windows.Forms;

namespace ConstructionTracker;

public class ReceiptDetailForm : Form
{
    public ReceiptDetailForm(Receipt receipt)
    {
        Text = $"Fiş Detayı - {receipt.ReceiptNo}";
        Width = 700;
        Height = 450;
        StartPosition = FormStartPosition.CenterParent;

        var l = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), RowCount = 2 };
        l.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        l.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        l.Controls.Add(new Label { Text = $"Fiş No: {receipt.ReceiptNo} | Yer: {receipt.Vendor} | Tarih: {receipt.Date:dd.MM.yyyy} | Tutar: {receipt.TotalAmount:N2}", AutoSize = true }, 0, 0);

        var g = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false, DataSource = receipt.Lines };
        l.Controls.Add(g, 0, 1);
        Controls.Add(l);
    }
}
