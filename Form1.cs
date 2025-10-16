using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace AnimalFeedApp
{
    public class Form1 : Form
    {
        private ComboBox cmbFeedType;
        private DateTimePicker dtpDate;
        private TextBox txtQuantity;
        private TextBox txtPrice;
        private Label lblTotals;
        private Button btnAdd, btnDelete, btnAddFeed, btnDeleteFeed, btnReport, btnDaily, btnMonthly;
        private DataGridView dgv;
        private SQLiteConnection conn;
        private Label lblTax;

        public Form1()
        {
            InitializeComponent();
            CreateDatabase();
            LoadFeedNames(); // تحميل أسماء الأعلاف من قاعدة البيانات
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "إدارة أعلاف الحيوانات";
            this.Font = new Font("Tahoma", 11);
            this.Size = new Size(950, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            Label lblDate = new Label { Text = "📅 التاريخ:", Left = 30, Top = 30, Width = 80 };
            dtpDate = new DateTimePicker { Left = 120, Top = 25, Width = 200 };

            Label lblFeed = new Label { Text = "🌾 نوع العلف:", Left = 350, Top = 30, Width = 100 };
            cmbFeedType = new ComboBox { Left = 460, Top = 25, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

            btnAddFeed = new Button { Text = "➕ إضافة علف", Left = 650, Top = 24, Width = 120, Height = 30, BackColor = Color.LightSkyBlue };
            btnAddFeed.Click += BtnAddFeed_Click;

            btnDeleteFeed = new Button { Text = "🗑️ حذف علف", Left = 780, Top = 24, Width = 120, Height = 30, BackColor = Color.IndianRed };
            btnDeleteFeed.Click += BtnDeleteFeed_Click;

            Label lblQuantity = new Label { Text = "⚖️ الكمية (قنطار):", Left = 30, Top = 80, Width = 140 };
            txtQuantity = new TextBox { Left = 180, Top = 75, Width = 150 };

            Label lblPrice = new Label { Text = "💰 سعر القنطار:", Left = 350, Top = 80, Width = 120 };
            txtPrice = new TextBox { Left = 480, Top = 75, Width = 150 };

            btnAdd = new Button { Text = "💾 إضافة", Left = 650, Top = 73, Width = 120, Height = 35, BackColor = Color.LightGreen };
            btnAdd.Click += BtnAdd_Click;

            // مربع البحث بالتاريخ
            Label lblSearch = new Label { Text = "🔍 بحث بالتاريخ:", Left = 30, Top = 120, Width = 120 };
            TextBox txtSearchDate = new TextBox { Left = 160, Top = 115, Width = 150 };
            Button btnSearch = new Button { Text = "بحث", Left = 320, Top = 113, Width = 80 };
            btnSearch.Click += (s, e) => LoadDataByDate(txtSearchDate.Text);

            dgv = new DataGridView
            {
                Left = 30,
                Top = 160,
                Width = 880,
                Height = 320,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 11, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.RowTemplate.Height = 28;
            dgv.ColumnHeadersHeight = 35;

            btnDelete = new Button { Text = "🗑️ حذف السطر المحدد", Left = 30, Top = 500, Width = 250, Height = 40, BackColor = Color.LightCoral };
            btnDelete.Click += BtnDelete_Click;

            lblTax = new Label { Text = "الضريبة: 0 دج", Left = 320, Top = 505, Width = 200, Font = new Font("Tahoma", 11, FontStyle.Bold) };
            lblTotals = new Label { Text = "الإجمالي: 0 دج", Left = 550, Top = 505, Width = 300, Font = new Font("Tahoma", 11, FontStyle.Bold) };

            btnDaily = new Button { Text = "📅 تقرير يومي", Left = 30, Top = 560, Width = 250, Height = 40, BackColor = Color.LightYellow };
            btnMonthly = new Button { Text = "🗓️ تقرير شهري", Left = 300, Top = 560, Width = 250, Height = 40, BackColor = Color.LightSalmon };
            btnDaily.Click += BtnDaily_Click;
            btnMonthly.Click += BtnMonthly_Click;

            btnReport = new Button { Text = "📊 كل السجلات", Left = 570, Top = 560, Width = 250, Height = 40, BackColor = Color.LightBlue };
            btnReport.Click += BtnReport_Click;

            Controls.AddRange(new Control[] {
                lblDate, dtpDate, lblFeed, cmbFeedType, btnAddFeed, btnDeleteFeed,
                lblQuantity, txtQuantity, lblPrice, txtPrice, btnAdd,
                lblSearch, txtSearchDate, btnSearch,
                dgv, btnDelete, lblTax, lblTotals,
                btnDaily, btnMonthly, btnReport
            });
        }

        private void CreateDatabase()
        {
            conn = new SQLiteConnection("Data Source=feeds.db;Version=3;");
            conn.Open();
            string sql = @"CREATE TABLE IF NOT EXISTS Feeds (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Date TEXT,
                            FeedType TEXT,
                            Quantity REAL,
                            Price REAL,
                            Total REAL
                        )";
            new SQLiteCommand(sql, conn).ExecuteNonQuery();

            string sqlFeeds = @"CREATE TABLE IF NOT EXISTS FeedNames (
                                 Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                 Name TEXT UNIQUE
                              )";
            new SQLiteCommand(sqlFeeds, conn).ExecuteNonQuery();
        }

        private void LoadFeedNames()
        {
            cmbFeedType.Items.Clear();
            string sql = "SELECT Name FROM FeedNames";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                cmbFeedType.Items.Add(reader["Name"].ToString());
            reader.Close();
        }

        private void BtnAddFeed_Click(object sender, EventArgs e)
        {
            string newFeed = Prompt("أدخل اسم العلف الجديد:", "إضافة نوع جديد");
            if (!string.IsNullOrWhiteSpace(newFeed))
            {
                SQLiteCommand cmd = new SQLiteCommand("INSERT OR IGNORE INTO FeedNames (Name) VALUES (@name)", conn);
                cmd.Parameters.AddWithValue("@name", newFeed.Trim());
                cmd.ExecuteNonQuery();
                LoadFeedNames();
                cmbFeedType.SelectedItem = newFeed.Trim();
            }
        }

        private void BtnDeleteFeed_Click(object sender, EventArgs e)
        {
            if (cmbFeedType.SelectedIndex == -1)
            {
                MessageBox.Show("يرجى اختيار نوع العلف المراد حذفه.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string feedToDelete = cmbFeedType.SelectedItem.ToString();
            DialogResult dr = MessageBox.Show("هل أنت متأكد أنك تريد حذف هذا العلف؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                SQLiteCommand cmd = new SQLiteCommand("DELETE FROM FeedNames WHERE Name=@name", conn);
                cmd.Parameters.AddWithValue("@name", feedToDelete);
                cmd.ExecuteNonQuery();
                LoadFeedNames();
            }
        }

        private string Prompt(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 180,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent
            };

            Label lblText = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true };
            TextBox txtInput = new TextBox() { Left = 20, Top = 50, Width = 340 };
            Button btnOk = new Button() { Text = "موافق", Left = 280, Width = 80, Top = 80, DialogResult = DialogResult.OK };

            prompt.Controls.Add(lblText);
            prompt.Controls.Add(txtInput);
            prompt.Controls.Add(btnOk);
            prompt.AcceptButton = btnOk;

            return prompt.ShowDialog() == DialogResult.OK ? txtInput.Text : "";
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbFeedType.SelectedIndex == -1 || string.IsNullOrWhiteSpace(txtQuantity.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("يرجى إدخال كل المعلومات", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double quantity, price;
            if (!double.TryParse(txtQuantity.Text, out quantity) || !double.TryParse(txtPrice.Text, out price))
            {
                MessageBox.Show("الرجاء إدخال أرقام صحيحة", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double total = quantity * price;
            string sql = "INSERT INTO Feeds (Date, FeedType, Quantity, Price, Total) VALUES (@d,@f,@q,@p,@t)";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@d", dtpDate.Value.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@f", cmbFeedType.Text);
            cmd.Parameters.AddWithValue("@q", quantity);
            cmd.Parameters.AddWithValue("@p", price);
            cmd.Parameters.AddWithValue("@t", total);
            cmd.ExecuteNonQuery();

            LoadData();
            txtQuantity.Clear();
            txtPrice.Clear();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null)
            {
                MessageBox.Show("يرجى تحديد السطر المراد حذفه.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult dr = MessageBox.Show("هل أنت متأكد من حذف هذا السطر؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.No) return;

            int id = Convert.ToInt32(dgv.CurrentRow.Cells["Id"].Value);
            string sql = "DELETE FROM Feeds WHERE Id=@id";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            LoadData();
        }

        private void LoadData(string filter = "")
        {
            string sql = "SELECT * FROM Feeds";
            if (!string.IsNullOrEmpty(filter))
                sql += " WHERE " + filter;

            SQLiteDataAdapter da = new SQLiteDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dgv.DataSource = dt;

            CalculateTotals(dt);
        }

        private void CalculateTotals(DataTable dt)
        {
            double total = 0;
            foreach (DataRow row in dt.Rows)
                total += Convert.ToDouble(row["Total"]);

            double tax = total * 0.05;
            lblTax.Text = $"الضريبة (5%): {tax:N2} دج";
            lblTotals.Text = $"الإجمالي الكلي: {total + tax:N2} دج";
        }

        private void LoadDataByDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
            {
                MessageBox.Show("الرجاء إدخال تاريخ للبحث.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoadData("Date = '" + date + "'");
        }

        private void BtnDaily_Click(object sender, EventArgs e)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            LoadData("Date = '" + today + "'");
        }

        private void BtnMonthly_Click(object sender, EventArgs e)
        {
            string month = DateTime.Now.ToString("yyyy-MM");
            LoadData("substr(Date,1,7) = '" + month + "'");
        }

        private void BtnReport_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
