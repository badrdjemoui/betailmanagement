using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AnimalFeedApp
{
    public class Form1 : Form
    {
        private ComboBox cmbFeedType;
        private DateTimePicker dtpDate;
        private TextBox txtQuantity, txtPrice;
        private Label lblTotals, lblTax, lblGrandTotal;
        private DataGridView dgv;

        private Button btnAdd, btnDelete, btnAddFeed, btnDeleteFeed, btnDaily, btnMonthly, btnReport;

        public Form1()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();
            FeedManager.LoadFeedNames(cmbFeedType);
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "إدارة أعلاف الحيوانات";
            this.Font = new Font("Tahoma", 11);
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            Label lblDate = new Label { Text = "📅 التاريخ:", Left = 30, Top = 30, Width = 80 };
            dtpDate = new DateTimePicker { Left = 120, Top = 25, Width = 200 };

            Label lblFeed = new Label { Text = "🌾 نوع العلف:", Left = 350, Top = 30, Width = 100 };
            cmbFeedType = new ComboBox { Left = 460, Top = 25, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

            btnAddFeed = new Button { Text = "➕ إضافة علف", Left = 650, Top = 24, Width = 120, Height = 30, BackColor = Color.LightSkyBlue };
            btnAddFeed.Click += (s, e) =>
            {
                string newFeed = Prompt("أدخل اسم العلف الجديد:", "إضافة نوع جديد");
                if (!string.IsNullOrWhiteSpace(newFeed))
                {
                    FeedManager.AddFeedName(newFeed);
                    FeedManager.LoadFeedNames(cmbFeedType);
                    cmbFeedType.SelectedItem = newFeed;
                }
            };

            btnDeleteFeed = new Button { Text = "🗑️ حذف علف", Left = 780, Top = 24, Width = 120, Height = 30, BackColor = Color.IndianRed };
            btnDeleteFeed.Click += (s, e) =>
            {
                if (cmbFeedType.SelectedIndex == -1)
                {
                    MessageBox.Show("يرجى اختيار نوع العلف المراد حذفه.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string feedToDelete = cmbFeedType.SelectedItem.ToString();
                if (MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FeedManager.DeleteFeedName(feedToDelete);
                    FeedManager.LoadFeedNames(cmbFeedType);
                }
            };

            Label lblQuantity = new Label { Text = "⚖️ الكمية:", Left = 30, Top = 80, Width = 140 };
            txtQuantity = new TextBox { Left = 180, Top = 75, Width = 150 };

            Label lblPrice = new Label { Text = "💰 السعر:", Left = 350, Top = 80, Width = 120 };
            txtPrice = new TextBox { Left = 480, Top = 75, Width = 150 };

            btnAdd = new Button { Text = "💾 إضافة", Left = 650, Top = 73, Width = 120, Height = 35, BackColor = Color.LightGreen };
            btnAdd.Click += BtnAdd_Click;

            // جدول البيانات
            dgv = new DataGridView
            {
                Left = 30,
                Top = 160,
                Width = 920,
                Height = 320,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            btnDelete = new Button { Text = "🗑️ حذف السطر المحدد", Left = 30, Top = 500, Width = 250, Height = 40, BackColor = Color.LightCoral };
            btnDelete.Click += BtnDelete_Click;

            lblTotals = new Label { Text = "الإجمالي بدون ضريبة: 0 دج", Left = 300, Top = 505, Width = 250 };
            lblTax = new Label { Text = "الضريبة (19%): 0 دج", Left = 580, Top = 505, Width = 200 };
            lblGrandTotal = new Label { Text = "الإجمالي مع الضريبة: 0 دج", Left = 780, Top = 505, Width = 250 };

            btnDaily = new Button { Text = "📅 تقرير يومي", Left = 30, Top = 560, Width = 250, Height = 40 };
            btnMonthly = new Button { Text = "🗓️ تقرير شهري", Left = 300, Top = 560, Width = 250, Height = 40 };
            btnReport = new Button { Text = "📊 كل السجلات", Left = 570, Top = 560, Width = 250, Height = 40 };

            // ✅ تعديل التقرير اليومي ليعرض بيانات التاريخ المختار من DateTimePicker
            btnDaily.Click += (s, e) =>
            {
                string selectedDate = dtpDate.Value.ToString("yyyy-MM-dd");
                LoadData("Date = '" + selectedDate + "'");
            };

            btnMonthly.Click += (s, e) => LoadData("substr(Date,1,7) = '" + DateTime.Now.ToString("yyyy-MM") + "'");
            btnReport.Click += (s, e) => LoadData();

            Controls.AddRange(new Control[] {
                lblDate, dtpDate, lblFeed, cmbFeedType, btnAddFeed, btnDeleteFeed,
                lblQuantity, txtQuantity, lblPrice, txtPrice, btnAdd,
                dgv, btnDelete, lblTotals, lblTax, lblGrandTotal,
                btnDaily, btnMonthly, btnReport
            });
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            double qty, price;
            if (cmbFeedType.SelectedIndex == -1 || !double.TryParse(txtQuantity.Text, out qty) || !double.TryParse(txtPrice.Text, out price))
            {
                MessageBox.Show("يرجى إدخال البيانات بشكل صحيح.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FeedManager.AddFeedRecord(dtpDate.Value.ToString("yyyy-MM-dd"), cmbFeedType.Text, qty, price);
            LoadData();
            txtQuantity.Clear();
            txtPrice.Clear();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("يرجى تحديد السطر للحذف.");
                return;
            }

            int id = Convert.ToInt32(dgv.CurrentRow.Cells["Id"].Value);
            FeedManager.DeleteFeedRecord(id);
            LoadData();
        }

        private void LoadData(string filter = "")
        {
            DataTable dt = FeedManager.LoadFeeds(filter);
            dgv.DataSource = dt;
            ReportManager.CalculateTotals(dt, lblTax, lblTotals, lblGrandTotal);
        }

        private string Prompt(string text, string caption)
        {
            Form prompt = new Form { Width = 400, Height = 180, Text = caption, StartPosition = FormStartPosition.CenterParent };
            Label lbl = new Label { Left = 20, Top = 20, Text = text, AutoSize = true };
            TextBox txt = new TextBox { Left = 20, Top = 50, Width = 340 };
            Button btn = new Button { Text = "موافق", Left = 280, Top = 80, Width = 80, DialogResult = DialogResult.OK };
            prompt.Controls.AddRange(new Control[] { lbl, txt, btn });
            prompt.AcceptButton = btn;
            return prompt.ShowDialog() == DialogResult.OK ? txt.Text : "";
        }
    }
}

