using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;


namespace AnimalFeedApp
{
    public class Form1 : Form
    {
        private ComboBox cmbFeedType;
        private DateTimePicker dtpDate;
        private TextBox txtQuantity, txtPrice;
        private Label lblTotals, lblTax, lblGrandTotal;
        private DataGridView dgv;

        private Button btnAdd, btnDelete, btnAddFeed, btnDeleteFeed, btnDaily, btnMonthly, btnReport, btnExportPDF;

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
            this.Font = new System.Drawing.Font("Tahoma", 11);
            this.Size = new Size(1000, 720);
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
            lblTax = new Label { Text = "الضريبة (5%): 0 دج", Left = 580, Top = 505, Width = 200 };
            lblGrandTotal = new Label { Text = "الإجمالي مع الضريبة: 0 دج", Left = 780, Top = 505, Width = 250 };

            btnDaily = new Button { Text = "📅 تقرير يومي", Left = 30, Top = 560, Width = 200, Height = 40 };
            btnMonthly = new Button { Text = "🗓️ تقرير شهري", Left = 240, Top = 560, Width = 200, Height = 40 };
            btnReport = new Button { Text = "📊 كل السجلات", Left = 450, Top = 560, Width = 200, Height = 40 };

            btnExportPDF = new Button { Text = "🖨️ طباعة تقرير PDF", Left = 660, Top = 560, Width = 250, Height = 40, BackColor = Color.LightYellow };
            btnExportPDF.Click += BtnExportPDF_Click;

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
                btnDaily, btnMonthly, btnReport, btnExportPDF
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

        private void BtnExportPDF_Click(object sender, EventArgs e)
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة.", "تنبيه");
                return;
            }

            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "تقرير الأعلاف.pdf");

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 30f, 30f, 30f, 30f);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    // تحميل خط عربي من النظام
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(bf, 14, iTextSharp.text.Font.NORMAL);
                    iTextSharp.text.Font arabicTitleFont = new iTextSharp.text.Font(bf, 22, iTextSharp.text.Font.BOLD);

                    // جدول صغير للعنوان والتاريخ لدعم RTL
                    PdfPTable headerTable = new PdfPTable(1);
                    headerTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    headerTable.WidthPercentage = 100;

                    PdfPCell titleCell = new PdfPCell(new Phrase("تقرير الأعلاف", arabicTitleFont));
                    titleCell.Border = PdfPCell.NO_BORDER;
                    titleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    titleCell.PaddingBottom = 10f;
                    headerTable.AddCell(titleCell);

                    PdfPCell dateCell = new PdfPCell(new Phrase($"التاريخ: {DateTime.Now:yyyy-MM-dd}", arabicFont));
                    dateCell.Border = PdfPCell.NO_BORDER;
                    dateCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    dateCell.PaddingBottom = 10f;
                    headerTable.AddCell(dateCell);

                    pdfDoc.Add(headerTable);

                    // خط فاصل
                    LineSeparator line = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -2);
                    pdfDoc.Add(new Chunk(line));
                    pdfDoc.Add(new Paragraph("\n")); // مسافة

                    // جدول البيانات
                    PdfPTable table = new PdfPTable(dgv.Columns.Count);
                    table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    table.WidthPercentage = 100;
                    table.SpacingBefore = 10f;
                    table.SpacingAfter = 10f;

                    // رؤوس الأعمدة بتنسيق جميل
                    foreach (DataGridViewColumn column in dgv.Columns)
                    {
                        PdfPCell headerCell = new PdfPCell(new Phrase(column.HeaderText, arabicFont));
                        headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        headerCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        headerCell.Padding = 5f;
                        table.AddCell(headerCell);
                    }

                    // بيانات الصفوف
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            PdfPCell dataCell = new PdfPCell(new Phrase(cell.Value?.ToString(), arabicFont));
                            dataCell.HorizontalAlignment = Element.ALIGN_CENTER;
                            dataCell.Padding = 5f;
                            table.AddCell(dataCell);
                        }
                    }

                    pdfDoc.Add(table);

                    // مسافة قبل المجاميع
                    pdfDoc.Add(new Paragraph("\n"));

                    // المجاميع داخل جدول صغير لدعم RTL
                    PdfPTable summaryTable = new PdfPTable(1);
                    summaryTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    summaryTable.WidthPercentage = 50;

                    PdfPCell totalCell = new PdfPCell(new Phrase(lblTotals.Text, arabicFont));
                    totalCell.Border = PdfPCell.NO_BORDER;
                    totalCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    totalCell.PaddingBottom = 5f;
                    summaryTable.AddCell(totalCell);

                    PdfPCell taxCell = new PdfPCell(new Phrase(lblTax.Text, arabicFont));
                    taxCell.Border = PdfPCell.NO_BORDER;
                    taxCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    taxCell.PaddingBottom = 5f;
                    summaryTable.AddCell(taxCell);

                    PdfPCell grandCell = new PdfPCell(new Phrase(lblGrandTotal.Text, arabicFont));
                    grandCell.Border = PdfPCell.NO_BORDER;
                    grandCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    summaryTable.AddCell(grandCell);

                    pdfDoc.Add(summaryTable);

                    pdfDoc.Close();
                }

                MessageBox.Show("تم إنشاء تقرير PDF على سطح المكتب بنجاح ✅", "نجاح");
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء إنشاء ملف PDF:\n" + ex.Message);
            }
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
