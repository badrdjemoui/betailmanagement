using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace AnimalFeedApp
{
    public static class FeedManager
    {
        public static void LoadFeedNames(ComboBox combo)
        {
            combo.Items.Clear();
            SQLiteCommand cmd = new SQLiteCommand("SELECT Name FROM FeedNames", DatabaseHelper.Connection);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                combo.Items.Add(reader["Name"].ToString());
            reader.Close();
        }

        public static void AddFeedName(string name)
        {
            SQLiteCommand cmd = new SQLiteCommand("INSERT OR IGNORE INTO FeedNames (Name) VALUES (@n)", DatabaseHelper.Connection);
            cmd.Parameters.AddWithValue("@n", name.Trim());
            cmd.ExecuteNonQuery();
        }

        public static void DeleteFeedName(string name)
        {
            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM FeedNames WHERE Name=@n", DatabaseHelper.Connection);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.ExecuteNonQuery();
        }

        public static void AddFeedRecord(string date, string type, double qty, double price)
        {
            double total = qty * price;
            SQLiteCommand cmd = new SQLiteCommand(
                "INSERT INTO Feeds (Date, FeedType, Quantity, Price, Total) VALUES (@d,@f,@q,@p,@t)",
                DatabaseHelper.Connection);
            cmd.Parameters.AddWithValue("@d", date);
            cmd.Parameters.AddWithValue("@f", type);
            cmd.Parameters.AddWithValue("@q", qty);
            cmd.Parameters.AddWithValue("@p", price);
            cmd.Parameters.AddWithValue("@t", total);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteFeedRecord(int id)
        {
            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Feeds WHERE Id=@id", DatabaseHelper.Connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public static DataTable LoadFeeds(string filter = "")
        {
            string sql = "SELECT * FROM Feeds";
            if (!string.IsNullOrEmpty(filter))
                sql += " WHERE " + filter;

            SQLiteDataAdapter da = new SQLiteDataAdapter(sql, DatabaseHelper.Connection);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
}
