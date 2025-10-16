using System.Data;
using System.Data.SQLite;

namespace AnimalFeedApp
{
    public static class DatabaseHelper
    {
        private static SQLiteConnection conn;

        public static SQLiteConnection Connection
        {
            get
            {
                if (conn == null)
                {
                    conn = new SQLiteConnection("Data Source=feeds.db;Version=3;");
                    conn.Open();
                }
                else if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                return conn;
            }
        }

        public static void InitializeDatabase()
        {
            SQLiteConnection c = Connection;

            string sqlFeeds = @"CREATE TABLE IF NOT EXISTS Feeds (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Date TEXT,
                                FeedType TEXT,
                                Quantity REAL,
                                Price REAL,
                                Total REAL
                              )";
            new SQLiteCommand(sqlFeeds, c).ExecuteNonQuery();

            string sqlFeedNames = @"CREATE TABLE IF NOT EXISTS FeedNames (
                                     Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                     Name TEXT UNIQUE
                                   )";
            new SQLiteCommand(sqlFeedNames, c).ExecuteNonQuery();
        }
    }
}
