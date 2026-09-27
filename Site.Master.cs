using System;
using System.Data.SQLite;
using Mini_Request_Management_Portal.Data;
using Mini_Request_Management_Portal.Models;

namespace Mini_Request_Management_Portal
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        private User CurrentUser => Session["CurrentUser"] as User;
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
                BindUserInfo();
        }

        private void BindUserInfo()
        {
            if (CurrentUser != null)
            {
                lblUserName.Text = CurrentUser.FullName;
                lblUserRole.Text = CurrentUser.Role;
            }
        }

        protected void btnSwitchRole_Click(object sender, EventArgs e)
        {
            string targetRole = (CurrentUser != null && CurrentUser.IsRequester)
                ? "Reviewer"
                : "Requester";

            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string sql = "SELECT Id, FullName, Role FROM Users " +
                             "WHERE Role = @Role ORDER BY Id LIMIT 1;";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Role", targetRole);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["CurrentUser"] = new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FullName = reader["FullName"].ToString(),
                                Role = reader["Role"].ToString()
                            };
                        }
                    }
                }
            }

            Response.Redirect(Request.RawUrl);
        }
    }
}