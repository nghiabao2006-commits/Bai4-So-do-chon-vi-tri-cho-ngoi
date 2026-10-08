using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bai4
{
    public partial class Form1 : Form
    {
        // UI state
        private TableLayoutPanel seatLayout;
        private Label lblSelectedCount;
        private Label lblSubtotal;
        private ComboBox cmbTimeSlot;
        private Button btnConfirm;
        private Button btnClearAll;

        private readonly int rows = 4;
        private readonly int cols = 5;
        private readonly Color EmptyColor = Color.WhiteSmoke;
        private readonly Color SelectedColor = Color.LightGreen;
        private readonly Color ReservedColor = Color.IndianRed;

        // map combobox options to prices
        private class SlotOption
        {
            public string Name { get; set; }
            public int Price { get; set; }
            public override string ToString() => $"{Name}: {Price:N0}đ";
        }

        public Form1()
        {
            InitializeComponent();
            InitializeDynamicUi();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // nothing extra for now
        }

        private void InitializeDynamicUi()
        {
            this.Text = "Interactive Slot Booking";
            this.ClientSize = new Size(900, 520);

            // Left: seats layout
            seatLayout = new TableLayoutPanel();
            seatLayout.Location = new Point(10, 10);
            seatLayout.Size = new Size(600, 480);
            seatLayout.ColumnCount = cols;
            seatLayout.RowCount = rows;
            seatLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            for (int c = 0; c < cols; c++)
                seatLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
            for (int r = 0; r < rows; r++)
                seatLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));

            // Create seat buttons
            int total = rows * cols;
            var reservedIndices = new HashSet<int> { 2, 6, 11 }; // some pre-reserved seats (0-based)
            for (int i = 0; i < total; i++)
            {
                var btn = new Button();
                btn.Dock = DockStyle.Fill;
                btn.Margin = new Padding(8);
                btn.Text = $"Seat {i + 1}";
                btn.Tag = i; // store index
                btn.BackColor = EmptyColor;
                btn.Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
                btn.Click += SeatButton_Click;

                if (reservedIndices.Contains(i))
                {
                    btn.BackColor = ReservedColor;
                    btn.Enabled = false;
                }

                int row = i / cols;
                int col = i % cols;
                seatLayout.Controls.Add(btn, col, row);
            }

            this.Controls.Add(seatLayout);

            // Right: controls panel
            var panel = new Panel();
            panel.Location = new Point(630, 10);
            panel.Size = new Size(250, 480);
            panel.BorderStyle = BorderStyle.None;

            var lbl1 = new Label();
            lbl1.Text = "Số vị trí đang chọn:";
            lbl1.Location = new Point(10, 10);
            lbl1.AutoSize = true;
            panel.Controls.Add(lbl1);

            lblSelectedCount = new Label();
            lblSelectedCount.Text = "0";
            lblSelectedCount.Location = new Point(10, 35);
            lblSelectedCount.AutoSize = true;
            lblSelectedCount.Font = new Font(lblSelectedCount.Font, FontStyle.Bold);
            panel.Controls.Add(lblSelectedCount);

            var lbl2 = new Label();
            lbl2.Text = "Tạm tính tiền:";
            lbl2.Location = new Point(10, 70);
            lbl2.AutoSize = true;
            panel.Controls.Add(lbl2);

            lblSubtotal = new Label();
            lblSubtotal.Text = "0đ";
            lblSubtotal.Location = new Point(10, 95);
            lblSubtotal.AutoSize = true;
            lblSubtotal.Font = new Font(lblSubtotal.Font, FontStyle.Bold);
            panel.Controls.Add(lblSubtotal);

            var lbl3 = new Label();
            lbl3.Text = "Khung giờ:";
            lbl3.Location = new Point(10, 135);
            lbl3.AutoSize = true;
            panel.Controls.Add(lbl3);

            cmbTimeSlot = new ComboBox();
            cmbTimeSlot.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTimeSlot.Location = new Point(10, 160);
            cmbTimeSlot.Width = 220;
            cmbTimeSlot.Items.Add(new SlotOption { Name = "Sáng", Price = 100000 });
            cmbTimeSlot.Items.Add(new SlotOption { Name = "Tối", Price = 150000 });
            cmbTimeSlot.SelectedIndex = 0;
            cmbTimeSlot.SelectedIndexChanged += CmbTimeSlot_SelectedIndexChanged;
            panel.Controls.Add(cmbTimeSlot);

            btnConfirm = new Button();
            btnConfirm.Text = "Xác nhận đặt";
            btnConfirm.Location = new Point(10, 220);
            btnConfirm.Width = 220;
            btnConfirm.Height = 40;
            btnConfirm.Click += BtnConfirm_Click;
            panel.Controls.Add(btnConfirm);

            btnClearAll = new Button();
            btnClearAll.Text = "Hủy chọn tất cả";
            btnClearAll.Location = new Point(10, 280);
            btnClearAll.Width = 220;
            btnClearAll.Height = 40;
            btnClearAll.Click += BtnClearAll_Click;
            panel.Controls.Add(btnClearAll);

            this.Controls.Add(panel);

            UpdateSummary();
        }

        private void SeatButton_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            // if reserved (disabled) do nothing
            if (!btn.Enabled) return;

            if (btn.BackColor == SelectedColor)
            {
                btn.BackColor = EmptyColor;
            }
            else
            {
                btn.BackColor = SelectedColor;
            }

            UpdateSummary();
        }

        private void CmbTimeSlot_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSummary();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            // mark selected seats as reserved
            foreach (Control c in seatLayout.Controls)
            {
                if (c is Button btn && btn.BackColor == SelectedColor)
                {
                    btn.BackColor = ReservedColor;
                    btn.Enabled = false;
                }
            }

            UpdateSummary();
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            // Reset all seats to empty and enable them (clear selections and reserved states)
            foreach (Control c in seatLayout.Controls)
            {
                if (c is Button btn)
                {
                    btn.BackColor = EmptyColor;
                    btn.Enabled = true;
                }
            }

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            int count = 0;
            foreach (Control c in seatLayout.Controls)
            {
                if (c is Button btn && btn.BackColor == SelectedColor)
                    count++;
            }

            lblSelectedCount.Text = count.ToString();

            var sel = cmbTimeSlot.SelectedItem as SlotOption;
            int price = sel?.Price ?? 0;
            long subtotal = (long)count * price;
            lblSubtotal.Text = subtotal.ToString("N0") + "đ";
        }
    }
}
