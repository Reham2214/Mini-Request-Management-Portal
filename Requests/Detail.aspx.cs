using Mini_Request_Management_Portal.Data;
using Mini_Request_Management_Portal.Models;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Mini_Request_Management_Portal.Requests
{
    public partial class Detail : Page
    {
        private User CurrentUser => Session["CurrentUser"] as User;

        private int CurrentRequestId
        {
            get { return ViewState["RequestId"] != null ? (int)ViewState["RequestId"] : 0; }
            set { ViewState["RequestId"] = value; }
        }

        private string CurrentStatus
        {
            get { return ViewState["CurrentStatus"] != null ? ViewState["CurrentStatus"].ToString() : string.Empty; }
            set { ViewState["CurrentStatus"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadRequestDetail();
        }

        private void LoadRequestDetail()
        {
            if (!int.TryParse(Request.QueryString["id"], out int requestId))
                return;
            CurrentRequestId = requestId;
            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                LoadRequest(connection, requestId);
                LoadHistory(connection, requestId);
                LoadActions(connection, requestId);
            }
        }

        private void LoadRequest(SQLiteConnection connection, int requestId)
        {
            string sql = @"
                SELECT r.Id, r.Title, r.Description, r.RequestType,
                       r.Department, r.Priority, r.Status,
                       r.AttachmentName, r.ReviewerComment,
                       r.AiSummary, r.AiSuggestedCategory, r.AiSuggestedPriority,
                       r.CreatedAt, r.ModifiedAt,
                       u.FullName AS CreatedByName
                FROM Requests r
                INNER JOIN Users u ON r.CreatedByUserId = u.Id
                WHERE r.Id = @Id;";

            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Id", requestId);

                using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    RequestCardView.DataSource = dt;
                    RequestCardView.DataBind();
                }
            }
        }

        private void LoadHistory(SQLiteConnection connection, int requestId)
        {
            string sql = @"
                SELECT 
                    Id,
                    Action, 
                    PerformedByName, 
                    OldStatus, 
                    NewStatus, 
                    PerformedAt
                FROM RequestHistory
                WHERE RequestId = @RequestId
                ORDER BY PerformedAt DESC;";

            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@RequestId", requestId);

                using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    HistoryGridView.DataSource = dt;
                    HistoryGridView.DataBind();
                }
            }
        }

        //  Load suitable next actions according to the current status of the selected request
        private void LoadActions(SQLiteConnection connection, int requestId)
        {
            if (CurrentUser == null)
                return;

            string sql = "SELECT Status FROM Requests WHERE Id = @Id;";

            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Id", requestId);
                string status = cmd.ExecuteScalar().ToString();
                CurrentStatus = status;

                if (CurrentUser.IsRequester)
                {
                    SubmitButton.Visible = status == RequestStatus.Draft;
                    CancelButton.Visible = status == RequestStatus.Draft;
                    EditButton.Visible = status == RequestStatus.Draft;
                }

                if (CurrentUser.IsReviewer)
                {
                    StartReviewButton.Visible = status == RequestStatus.Submitted;
                    CompleteButton.Visible = status == RequestStatus.InReview;
                    CancelButton.Visible = status == RequestStatus.Submitted
                                             || status == RequestStatus.InReview;
                }
            }
        }

        // Add new record to the History table
        private void InsertHistory(SQLiteConnection connection, int requestId, string action, User performedBy, string oldStatus, string newStatus)
        {
            string historySql = @"
        INSERT INTO RequestHistory
            (RequestId, Action, PerformedByUserId,
             PerformedByName, OldStatus, NewStatus,
             PerformedAt)
        VALUES
            (@RequestId, @Action, @PerformedByUserId,
             @PerformedByName, @OldStatus, @NewStatus,
             @PerformedAt);";

            using (SQLiteCommand cmd = new SQLiteCommand(historySql, connection))
            {
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@PerformedByUserId", performedBy.Id);
                cmd.Parameters.AddWithValue("@PerformedByName", performedBy.FullName);
                cmd.Parameters.AddWithValue("@OldStatus", string.IsNullOrEmpty(oldStatus) ? (object)DBNull.Value : oldStatus);
                cmd.Parameters.AddWithValue("@NewStatus", string.IsNullOrEmpty(newStatus) ? (object)DBNull.Value : newStatus);
                cmd.Parameters.AddWithValue("@PerformedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
        }

        // Workflow action has moved forward.
        private void ChangeStatus(string newStatus, string comment = null)
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
                        string updateSql = @"
                    UPDATE Requests SET
                        Status          = @Status,
                        ReviewerComment = COALESCE(@ReviewerComment, ReviewerComment),
                        ModifiedAt      = @ModifiedAt
                    WHERE Id = @Id;";

                        using (SQLiteCommand cmd = new SQLiteCommand(updateSql, connection))
                        {
                            cmd.Parameters.AddWithValue("@Status", newStatus);
                            cmd.Parameters.AddWithValue("@ReviewerComment", string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment);
                            cmd.Parameters.AddWithValue("@ModifiedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@Id", CurrentRequestId);
                            cmd.ExecuteNonQuery();
                        }

                        InsertHistory(connection, CurrentRequestId, "Status changed", CurrentUser, CurrentStatus, newStatus);

                        if (!string.IsNullOrEmpty(comment))
                            InsertHistory(connection, CurrentRequestId, "Reviewer comment added",
                                CurrentUser, CurrentStatus, newStatus);

                        transaction.Commit();

                        CurrentStatus = newStatus;

                        using (SQLiteConnection refreshConnection = DatabaseHelper.GetConnection())
                        {
                            refreshConnection.Open();
                            LoadRequest(refreshConnection, CurrentRequestId);
                            LoadHistory(refreshConnection, CurrentRequestId);
                            LoadActions(refreshConnection, CurrentRequestId);
                            SuccessPanel.Visible = true;
                            SuccessMessage.Text = "Request status changed successfully.";
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        //  Submit request button clicked
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            ChangeStatus(RequestStatus.Submitted);
        }

        //  Cancel request button clicked
        protected void CancelButton_Click(object sender, EventArgs e)
        {
            ChangeStatus(RequestStatus.Cancelled);
        }

        // Reviewer clicked the save button to submit the comment.
        protected void SaveCommentButton_Click(object sender, EventArgs e)
        {
            ChangeStatus(PendingStatusHidden.Value, ReviewerCommentMemo.Text.Trim());
            ReviewerCommentPopup.ShowOnPageLoad = false;
        }

        //  Complete request button clicked
        protected void CompleteButton_Click(object sender, EventArgs e)
        {
            PendingStatusHidden.Value = RequestStatus.Completed;
            ReviewerCommentMemo.Text = string.Empty;
            ReviewerCommentPopup.ShowOnPageLoad = true;
        }

        //  Review request button clicked
        protected void StartReviewButton_Click(object sender, EventArgs e)
        {
            PendingStatusHidden.Value = RequestStatus.InReview;
            ReviewerCommentMemo.Text = string.Empty;
            ReviewerCommentPopup.ShowOnPageLoad = true;
        }

        //  Generate AI suggestions button clicked
        protected void EditGenerateAIButton_Click(object sender, EventArgs e)
        {
            string title = EditTitleTextBox.Text.Trim();
            string description = EditDescriptionMemo.Text.Trim();

            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(description))
            {
                EditValidationPanel.Visible = true;
                EditValidationMessage.Text = "Please enter a title and description before generating AI suggestions.";
                return;
            }

            try
            {
                AiSuggestionResult result = Task.Run(() => CallGitHubModelsApi(title, description)).Result;

                if (result != null)
                {
                    EditAiSummaryLiteral.Text = result.Summary;
                    EditAiCategoryLiteral.Text = result.SuggestedCategory;
                    EditAiPriorityLiteral.Text = result.SuggestedPriority;

                    EditAiSummaryHidden.Value = result.Summary;
                    EditAiCategoryHidden.Value = result.SuggestedCategory;
                    EditAiPriorityHidden.Value = result.SuggestedPriority;

                    EditAISuggestionsPanel.Visible = true;
                    if (CurrentUser != null)
                    {
                        using (SQLiteConnection connection = DatabaseHelper.GetConnection())
                        {
                            connection.Open();
                            InsertHistory(connection, CurrentRequestId, "AI suggestions generated",
                                CurrentUser, null, null);
                        }
                    }

                    if (EditPriorityComboBox.Value == null || string.IsNullOrEmpty(EditPriorityComboBox.Value.ToString()))
                        EditPriorityComboBox.Value = result.SuggestedPriority;

                    EditRequestPopupControl.ShowOnPageLoad = true;
                }
            }
            catch
            {
                EditValidationPanel.Visible = true;
                EditValidationMessage.Text = "AI suggestion failed. You can continue without it.";
                EditRequestPopupControl.ShowOnPageLoad = true;
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

        //  Edit request button clicked
        protected void EditButton_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string sql = @"
            SELECT Title, Description, RequestType,
                   Department, Priority, AttachmentName
            FROM Requests
            WHERE Id = @Id;";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", CurrentRequestId);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            EditTitleTextBox.Text = reader["Title"].ToString();
                            EditDescriptionMemo.Text = reader["Description"].ToString();
                            EditRequestTypeComboBox.Value = reader["RequestType"].ToString();
                            EditDepartmentComboBox.Value = reader["Department"].ToString();
                            EditPriorityComboBox.Value = reader["Priority"].ToString();
                            EditAttachmentTextBox.Text = reader["AttachmentName"] == DBNull.Value
                                                             ? string.Empty
                                                             : reader["AttachmentName"].ToString();
                        }
                    }
                }
            }

            EditValidationPanel.Visible = false;
            EditRequestPopupControl.ShowOnPageLoad = true;
        }

        //  Update request button clicked
        protected void SaveEditButton_Click(object sender, EventArgs e)
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
                        string updateSql = @"
                        UPDATE Requests SET
                            Title               = @Title,
                            Description         = @Description,
                            RequestType         = @RequestType,
                            Department          = @Department,
                            Priority            = @Priority,
                            AttachmentName      = @AttachmentName,
                            AiSummary           = COALESCE(@AiSummary, AiSummary),
                            AiSuggestedCategory = COALESCE(@AiSuggestedCategory, AiSuggestedCategory),
                            AiSuggestedPriority = COALESCE(@AiSuggestedPriority, AiSuggestedPriority),
                            ModifiedAt          = @ModifiedAt
                        WHERE Id = @Id;";

                        using (SQLiteCommand cmd = new SQLiteCommand(updateSql, connection))
                        {
                            cmd.Parameters.AddWithValue("@Title", EditTitleTextBox.Text.Trim());
                            cmd.Parameters.AddWithValue("@Description", EditDescriptionMemo.Text.Trim());
                            cmd.Parameters.AddWithValue("@RequestType", EditRequestTypeComboBox.Value.ToString());
                            cmd.Parameters.AddWithValue("@Department", EditDepartmentComboBox.Value.ToString());
                            cmd.Parameters.AddWithValue("@Priority", EditPriorityComboBox.Value == null || string.IsNullOrEmpty(EditPriorityComboBox.Value.ToString()) ? "Medium" : EditPriorityComboBox.Value.ToString());
                            cmd.Parameters.AddWithValue("@AttachmentName", string.IsNullOrEmpty(EditAttachmentTextBox.Text) ? (object)DBNull.Value : EditAttachmentTextBox.Text.Trim());
                            cmd.Parameters.AddWithValue("@AiSummary", string.IsNullOrEmpty(EditAiSummaryHidden.Value) ? (object)DBNull.Value : EditAiSummaryHidden.Value);
                            cmd.Parameters.AddWithValue("@AiSuggestedCategory", string.IsNullOrEmpty(EditAiCategoryHidden.Value) ? (object)DBNull.Value : EditAiCategoryHidden.Value);
                            cmd.Parameters.AddWithValue("@AiSuggestedPriority", string.IsNullOrEmpty(EditAiPriorityHidden.Value) ? (object)DBNull.Value : EditAiPriorityHidden.Value);
                            cmd.Parameters.AddWithValue("@ModifiedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@Id", CurrentRequestId);
                            cmd.ExecuteNonQuery();
                        }

                        InsertHistory(connection, CurrentRequestId, "Request edited",CurrentUser, CurrentStatus, CurrentStatus);

                        transaction.Commit();

                        EditRequestPopupControl.ShowOnPageLoad = false;

                        using (SQLiteConnection refreshConnection = DatabaseHelper.GetConnection())
                        {
                            refreshConnection.Open();
                            LoadRequest(refreshConnection, CurrentRequestId);
                            LoadHistory(refreshConnection, CurrentRequestId);
                            LoadActions(refreshConnection, CurrentRequestId);
                        }

                        SuccessPanel.Visible = true;
                        SuccessMessage.Text = "Request updated successfully.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        EditValidationPanel.Visible = true;
                        EditValidationMessage.Text = "An error occurred: " + ex.Message;
                    }
                }
            }
        }

    }
}