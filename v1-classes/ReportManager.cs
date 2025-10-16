using System;
using System.Data;
using System.Windows.Forms;

namespace AnimalFeedApp
{
    public static class ReportManager
    {
        public static void CalculateTotals(DataTable dt, Label lblTax, Label lblTotals, Label lblGrandTotal)
        {
            double total = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (double.TryParse(row["Quantity"].ToString(), out double qty) &&
                    double.TryParse(row["Price"].ToString(), out double price))
                {
                    total += qty * price;
                }
            }

            double taxRate = 0.19; // 19% ضريبة
            double tax = total * taxRate;
            double grandTotal = total + tax;

            lblTotals.Text = $"الإجمالي بدون ضريبة: {total:0.00} دج";
            lblTax.Text = $"الضريبة (19%): {tax:0.00} دج";
            lblGrandTotal.Text = $"الإجمالي مع الضريبة: {grandTotal:0.00} دج";
        }
    }
}

