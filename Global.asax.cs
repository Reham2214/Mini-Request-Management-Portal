using Mini_Request_Management_Portal.Data;
using Mini_Request_Management_Portal.Models;
using System;
using System.Data.SQLite;
using System.Web;
using System.Web.UI;

namespace Mini_Request_Management_Portal
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            DatabaseHelper.InitialiseDatabase();
            ScriptManager.ScriptResourceMapping.AddDefinition("jquery",
                new ScriptResourceDefinition
                {
                    Path = "~/Scripts/jquery-3.7.0.min.js",
                    DebugPath = "~/Scripts/jquery-3.7.0.js",
                    CdnPath = "https://cdn.jsdelivr.net/npm/jquery@3.7.0/dist/jquery.min.js"
                });
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Set default user when a new session starts
            // This runs before any page loads
            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string sql = "SELECT Id, FullName, Role " +
                             "FROM Users WHERE Role = 'Requester' " +
                             "ORDER BY Id LIMIT 1;";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Session["CurrentUser"] = new User
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            FullName = reader["FullName"].ToString(),
                            Role = reader["Role"].ToString(),
                        };
                    }
                }
            }
        }
    }
}