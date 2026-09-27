using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Web.UI;
using DevExpress.Web;
using Mini_Request_Management_Portal.Data;
using Mini_Request_Management_Portal.Models;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mini_Request_Management_Portal
{
    public partial class _Default : Page
    {
        private User CurrentUser => Session["CurrentUser"] as User;
        protected void Page_Load(object sender, EventArgs e)
        {

            if (CurrentUser == null)
                return;

            if (!IsPostBack)
                ConfigurePageForRole();

            BindGrid();
        }

        //  Show (add new request button) only for Requesters
        private void ConfigurePageForRole()
        {
            if (CurrentUser != null && CurrentUser.IsRequester)
                CreateNewRequestButton.Visible = true;
        }

        //  Bind grid based on role
        private void BindGrid()
        {
            if (CurrentUser == null)
                return;

            List<RequestViewModel> requests = CurrentUser.IsReviewer
                ? GetAllRequests()
                : GetRequestsByUser(CurrentUser.Id);

            gvRequests.DataSource = requests;
            gvRequests.DataBind();
        }

        //  Get all requests (Reviewer)
        private List<RequestViewModel> GetAllRequests()
        {
            List<RequestViewModel> requests = new List<RequestViewModel>();

            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string sql = @"
                SELECT r.Id, r.Title, r.RequestType, r.Department,
                       r.Priority, r.Status, r.CreatedAt,
                       u.FullName AS CreatedByName
                FROM Requests r
                INNER JOIN Users u ON r.CreatedByUserId = u.Id
                WHERE r.Status <> 'Draft'
                AND NOT (
                    r.Status = 'Cancelled'
                    AND EXISTS (
                        SELECT 1 FROM RequestHistory h
                        WHERE h.RequestId = r.Id
                        AND h.OldStatus = 'Draft'
                        AND h.NewStatus = 'Cancelled'
                    )
                    AND NOT EXISTS (
                        SELECT 1 FROM RequestHistory h
                        WHERE h.RequestId = r.Id
                        AND h.OldStatus = 'Submitted'
                    )
                )
                ORDER BY r.CreatedAt DESC;";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        requests.Add(MapViewModel(reader));
                }
            }

            return requests;
        }

        //  Get requests by user (Requester)
        private List<RequestViewModel> GetRequestsByUser(int userId)
        {
            List<RequestViewModel> requests = new List<RequestViewModel>();

            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string sql = @"
                    SELECT r.Id, r.Title, r.RequestType, r.Department,
                           r.Priority, r.Status, r.CreatedAt,
                           u.FullName AS CreatedByName
                    FROM Requests r
                    INNER JOIN Users u ON r.CreatedByUserId = u.Id
                    WHERE r.CreatedByUserId = @UserId
                    ORDER BY r.CreatedAt DESC;";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            requests.Add(MapViewModel(reader));
                    }
                }
            }

            return requests;
        }

        //  Map reader to view model
        private RequestViewModel MapViewModel(SQLiteDataReader reader)
        {
            return new RequestViewModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Title = reader["Title"].ToString(),
                RequestType = reader["RequestType"].ToString(),
                Department = reader["Department"].ToString(),
                Priority = reader["Priority"].ToString(),
                Status = reader["Status"].ToString(),
                CreatedByName = reader["CreatedByName"].ToString(),
                CreatedAt = reader["CreatedAt"].ToString()
            };
        }

        //  Submit request button clicked
        protected void SubmitRequestButton_Click(object sender, EventArgs e)
        {
            SaveRequest(RequestStatus.Submitted);
        }

        //  Save as draft button clicked
        protected void SaveDraftButton_Click(object sender, EventArgs e)
        {
            SaveRequest(RequestStatus.Draft);
        }

        //  Store the request data to the database either as a submission or a draft
        private void SaveRequest(string status)
        {
            if (CurrentUser == null)
                return;

            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertRequestSql = @"
                    INSERT INTO Requests
                        (Title, Description, RequestType, Department,
                         Priority, Status, CreatedByUserId,
                         AttachmentName, AiSummary, AiSuggestedCategory,
                         AiSuggestedPriority, CreatedAt, ModifiedAt)
                    VALUES
                        (@Title, @Description, @RequestType, @Department,
                         @Priority, @Status, @CreatedByUserId,
                         @AttachmentName, @AiSummary, @AiSuggestedCategory,
                         @AiSuggestedPriority, @CreatedAt, @ModifiedAt);
                    SELECT last_insert_rowid();";

                        int newRequestId;

                        using (SQLiteCommand command = new SQLiteCommand(insertRequestSql, connection))
                        {
                            command.Parameters.AddWithValue("@Title", TitleTextBox.Text.Trim());
                            command.Parameters.AddWithValue("@Description", DescriptionMemo.Text.Trim());
                            command.Parameters.AddWithValue("@RequestType", RequestTypeComboBox.Value == null ? string.Empty : RequestTypeComboBox.Value.ToString());
                            command.Parameters.AddWithValue("@Department", DepartmentComboBox.Value == null ? string.Empty : DepartmentComboBox.Value.ToString());
                            command.Parameters.AddWithValue("@Priority", PriorityComboBox.Value == null || string.IsNullOrEmpty(PriorityComboBox.Value.ToString()) ? "Medium" : PriorityComboBox.Value.ToString());
                            command.Parameters.AddWithValue("@Status", status);
                            command.Parameters.AddWithValue("@CreatedByUserId", CurrentUser.Id);
                            command.Parameters.AddWithValue("@AttachmentName", string.IsNullOrEmpty(AttachmentTextBox.Text) ? (object)DBNull.Value : AttachmentTextBox.Text.Trim());
                            command.Parameters.AddWithValue("@AiSummary", string.IsNullOrEmpty(AiSummaryHidden.Value) ? (object)DBNull.Value : AiSummaryHidden.Value);
                            command.Parameters.AddWithValue("@AiSuggestedCategory", string.IsNullOrEmpty(AiCategoryHidden.Value) ? (object)DBNull.Value : AiCategoryHidden.Value);
                            command.Parameters.AddWithValue("@AiSuggestedPriority", string.IsNullOrEmpty(AiPriorityHidden.Value) ? (object)DBNull.Value : AiPriorityHidden.Value);
                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            command.Parameters.AddWithValue("@ModifiedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            newRequestId = Convert.ToInt32(command.ExecuteScalar());
                        }

                        string insertHistorySql = @"
                    INSERT INTO RequestHistory
                        (RequestId, Action, PerformedByUserId,
                         PerformedByName, OldStatus, NewStatus,
                         PerformedAt)
                    VALUES
                        (@RequestId, @Action, @PerformedByUserId,
                         @PerformedByName, @OldStatus, @NewStatus,
                         @PerformedAt);";

                        using (SQLiteCommand command = new SQLiteCommand(insertHistorySql, connection))
                        {
                            command.Parameters.AddWithValue("@RequestId", newRequestId);
                            command.Parameters.AddWithValue("@Action", "Request created");
                            command.Parameters.AddWithValue("@PerformedByUserId", CurrentUser.Id);
                            command.Parameters.AddWithValue("@PerformedByName", CurrentUser.FullName);
                            command.Parameters.AddWithValue("@OldStatus", DBNull.Value);
                            command.Parameters.AddWithValue("@NewStatus", status);
                            command.Parameters.AddWithValue("@PerformedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                            command.ExecuteNonQuery();
                        }
                        if (!string.IsNullOrEmpty(AiSummaryHidden.Value))
                        {
                            using (SQLiteCommand command = new SQLiteCommand(insertHistorySql, connection))
                            {
                                command.Parameters.AddWithValue("@RequestId", newRequestId);
                                command.Parameters.AddWithValue("@Action", "AI suggestions generated");
                                command.Parameters.AddWithValue("@PerformedByUserId", CurrentUser.Id);
                                command.Parameters.AddWithValue("@PerformedByName", CurrentUser.FullName);
                                command.Parameters.AddWithValue("@OldStatus", DBNull.Value);
                                command.Parameters.AddWithValue("@NewStatus", DBNull.Value);
                                command.Parameters.AddWithValue("@PerformedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();

                        TitleTextBox.Text = string.Empty;
                        DescriptionMemo.Text = string.Empty;
                        RequestTypeComboBox.Value = null;
                        DepartmentComboBox.Value = null;
                        PriorityComboBox.Value = null;
                        AttachmentTextBox.Text = string.Empty;
                        AiSummaryHidden.Value = string.Empty;
                        AiCategoryHidden.Value = string.Empty;
                        AiPriorityHidden.Value = string.Empty;
                        AISuggestionsPanel.Visible = false;
                        ValidationPanel.Visible = false;

                        SuccessPanel.Visible = true;
                        SuccessMessage.Text = status == RequestStatus.Draft
                            ? "Request saved as draft successfully."
                            : "Request submitted successfully.";

                        BindGrid();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ValidationPanel.Visible = true;
                        ValidationMessage.Text = "An error occurred: " + ex.Message;
                    }
                }
            }
        }

        //  Generate AI suggestions button clicked
        protected void GenerateAISuggestionsButton_Click(object sender, EventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string description = DescriptionMemo.Text.Trim();

            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(description))
            {
                ValidationPanel.Visible = true;
                ValidationMessage.Text = "Please enter a title and description before generating AI suggestions.";
                return;
            }

            try
            {
                AiSuggestionResult result = Task.Run(() => CallGitHubModelsApi(title, description)).Result;

                if (result != null)
                {
                    AiSummaryLiteral.Text = result.Summary;
                    AiCategoryLiteral.Text = result.SuggestedCategory;
                    AiPriorityLiteral.Text = result.SuggestedPriority;

                    AiSummaryHidden.Value = result.Summary;
                    AiCategoryHidden.Value = result.SuggestedCategory;
                    AiPriorityHidden.Value = result.SuggestedPriority;

                    AISuggestionsPanel.Visible = true;

                    if (PriorityComboBox.Value == null || string.IsNullOrEmpty(PriorityComboBox.Value.ToString()))
                        PriorityComboBox.Value = result.SuggestedPriority;
                }
            }
            catch
            {
                ValidationPanel.Visible = true;
                ValidationMessage.Text = "AI suggestion failed. You can continue without it.";
            }
        }

        private async Task<AiSuggestionResult> CallGitHubModelsApi(string title, string description)
        {
            string token = "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";
            string url = "https://models.inference.ai.azure.com/chat/completions";

            using (var client = new System.Net.Http.HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                new
                {
                    role    = "system",
                    content = "You are an assistant for an internal request management portal. Given a request title and description, return ONLY a JSON object with exactly these keys: summary (max 2 sentences), suggestedCategory (one of: Access Request, Change Request, Incident, Information Request, Data Update Request), suggestedPriority (one of: Low, Medium, High). No markdown, no code fences, no extra text."
                },
                new
                {
                    role    = "user",
                    content = $"Title: {title}\nDescription: {description}"
                }
            }
                };

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                string resJson = await response.Content.ReadAsStringAsync();

                dynamic parsed = JsonConvert.DeserializeObject(resJson);
                string text = parsed.choices[0].message.content.ToString();
                string cleaned = text.Replace("```json", "").Replace("```", "").Trim();

                return JsonConvert.DeserializeObject<AiSuggestionResult>(cleaned);
            }
        }

    }
}