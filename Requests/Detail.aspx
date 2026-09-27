<%@ Page Title="Request Detail" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="Detail.aspx.cs"
    Inherits="Mini_Request_Management_Portal.Requests.Detail" %>

<%@ Register Assembly="DevExpress.Web.Bootstrap.v25.2, Version=25.2.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a"
    Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Remove card border */
        .dxbs-card {
            border: none !important;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

        <!-- Back button -->
        <div class="mb-3">
            <dx:BootstrapButton ID="BackButton" runat="server"
                Text="← Back to Requests"
                CssClasses-Control="btn btn-secondary"
                AutoPostBack="false"
                ClientSideEvents-Click="function(s, e) { window.location.href = '/'; }" />
        </div>

        <!-- Request details -->
        <dx:BootstrapCardView ID="RequestCardView" runat="server"
            KeyFieldName="Id">
            <SettingsLayout CardColSpanLg="12" />
            <Columns>
                <dx:BootstrapCardViewColumn FieldName="Title" Caption="Title" />
                <dx:BootstrapCardViewColumn FieldName="Description" Caption="Description" />
                <dx:BootstrapCardViewColumn FieldName="RequestType" Caption="Request Type" />
                <dx:BootstrapCardViewColumn FieldName="Department" Caption="Department" />

                <dx:BootstrapCardViewColumn FieldName="Status" Caption="Status" />
                <dx:BootstrapCardViewColumn FieldName="Priority" Caption="Priority" />
                <dx:BootstrapCardViewColumn FieldName="AiSummary" Caption="AI Summary" />
                <dx:BootstrapCardViewColumn FieldName="AiSuggestedCategory" Caption="AI Suggested Category" />
                <dx:BootstrapCardViewColumn FieldName="AiSuggestedPriority" Caption="AI Suggested Priority" />
                <dx:BootstrapCardViewColumn FieldName="AttachmentName" Caption="Attachment" />
                <dx:BootstrapCardViewColumn FieldName="CreatedByName" Caption="Created By" />
                <dx:BootstrapCardViewColumn FieldName="CreatedAt" Caption="Created Date" />
                <dx:BootstrapCardViewColumn FieldName="ModifiedAt" Caption="Last Modified" />
                <dx:BootstrapCardViewColumn FieldName="ReviewerComment" Caption="Reviewer Comment" />
            </Columns>
        </dx:BootstrapCardView>

        <!-- Actions -->
        <div class="mt-4">
            <h4 class="mb-3">Actions</h4>
            <dx:BootstrapButton ID="SubmitButton" runat="server"
                Text="Submit" Visible="false"
                OnClick="SubmitButton_Click" />

            <dx:BootstrapButton ID="EditButton" runat="server"
                Text="Edit" Visible="false" CausesValidation="false"
                OnClick="EditButton_Click" />

            <dx:BootstrapButton ID="StartReviewButton" runat="server"
                Text="Start Review" Visible="false"
                OnClick="StartReviewButton_Click" />

            <dx:BootstrapButton ID="CompleteButton" runat="server"
                Text="Complete" Visible="false"
                OnClick="CompleteButton_Click" />

            <dx:BootstrapButton ID="CancelButton" runat="server"
                Text="Cancel" Visible="false"
                OnClick="CancelButton_Click" />
        </div>
        
        <!-- Save currunt status in a hiddin field -->
        <asp:HiddenField ID="PendingStatusHidden" runat="server" />
        
        <!-- Success message -->
        <asp:Panel ID="SuccessPanel" runat="server"
            Visible="false"
            CssClass="alert alert-success mt-2">
            <asp:Literal ID="SuccessMessage" runat="server" />
        </asp:Panel>

        <!--  Reviewer comment -->
        <dx:BootstrapPopupControl ID="ReviewerCommentPopup" runat="server"
            ShowOnPageLoad="false"
            ClientInstanceName="reviewerCommentPopup"
            PopupHorizontalAlign="WindowCenter"
            PopupVerticalAlign="WindowCenter"
            Width="500px"
            CloseAction="CloseButton"
            HeaderText="Reviewer Comment">
            <ContentCollection>
                <dx:ContentControl runat="server">
                    <dx:BootstrapFormLayout ID="CommentFormLayout" runat="server">
                        <Items>
                            <dx:BootstrapLayoutItem Caption="Comment" ColSpanMd="12">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <dx:BootstrapMemo ID="ReviewerCommentMemo" runat="server"
                                            Rows="4"
                                            Width="100%"
                                            NullText="Enter reviewer comment..." />
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>
                        </Items>
                    </dx:BootstrapFormLayout>
                    <div class="d-flex justify-content-end p-3 border-top">
                        <dx:BootstrapButton ID="SaveCommentButton" runat="server"
                            Text="Save and Continue"
                            SettingsBootstrap-RenderOption="Secondary"
                            OnClick="SaveCommentButton_Click" />
                    </div>
                </dx:ContentControl>
            </ContentCollection>
        </dx:BootstrapPopupControl>

        <!--  Edit request -->
        <dx:BootstrapPopupControl runat="server"
            ShowOnPageLoad="false"
            ID="EditRequestPopupControl"
            ClientInstanceName="editRequestPopupControl"
            PopupHorizontalAlign="WindowCenter"
            PopupVerticalAlign="WindowCenter"
            Width="800px"
            CloseAction="CloseButton"
            HeaderText="Edit Request">
            <ContentCollection>
                <dx:ContentControl runat="server">

                    <dx:BootstrapFormLayout ID="EditRequestFormLayout" runat="server">
                        <Items>

                            <dx:BootstrapLayoutItem Caption="Title" ColSpanMd="12">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <dx:BootstrapTextBox ID="EditTitleTextBox" runat="server"
                                            NullText="Enter request title..."
                                            Width="100%"
                                            ValidationSettings-RequiredField-IsRequired="true"
                                            ValidationSettings-RequiredField-ErrorText="Title is required." />
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>

                            <dx:BootstrapLayoutItem Caption="Description" ColSpanMd="12">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <dx:BootstrapMemo ID="EditDescriptionMemo" runat="server"
                                            NullText="Describe your request in detail..."
                                            Rows="4"
                                            Width="100%"
                                            ValidationSettings-RequiredField-IsRequired="true"
                                            ValidationSettings-RequiredField-ErrorText="Description is required." />
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>

                            <dx:BootstrapLayoutItem Caption="Request Type" ColSpanMd="6">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <dx:BootstrapComboBox ID="EditRequestTypeComboBox" runat="server"
                                            NullText="Select type..."
                                            Width="100%"
                                            ValidationSettings-RequiredField-IsRequired="true"
                                            ValidationSettings-RequiredField-ErrorText="Request Type is required.">
                                            <Items>
                                                <dx:BootstrapListEditItem Text="Access Request" Value="Access Request" />
                                                <dx:BootstrapListEditItem Text="Change Request" Value="Change Request" />
                                                <dx:BootstrapListEditItem Text="Incident" Value="Incident" />
                                                <dx:BootstrapListEditItem Text="Information Request" Value="Information Request" />
                                                <dx:BootstrapListEditItem Text="Data Update Request" Value="Data Update Request" />
                                            </Items>
                                        </dx:BootstrapComboBox>
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>

                            <dx:BootstrapLayoutItem Caption="Department" ColSpanMd="6">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <dx:BootstrapComboBox ID="EditDepartmentComboBox" runat="server"
                                            NullText="Select department..."
                                            Width="100%"
                                            ValidationSettings-RequiredField-IsRequired="true"
                                            ValidationSettings-RequiredField-ErrorText="Department is required.">
                                            <Items>
                                                <dx:BootstrapListEditItem Text="IT" Value="IT" />
                                                <dx:BootstrapListEditItem Text="HR" Value="HR" />
                                                <dx:BootstrapListEditItem Text="Finance" Value="Finance" />
                                                <dx:BootstrapListEditItem Text="Operations" Value="Operations" />
                                                <dx:BootstrapListEditItem Text="Legal" Value="Legal" />
                                                <dx:BootstrapListEditItem Text="Marketing" Value="Marketing" />
                                            </Items>
                                        </dx:BootstrapComboBox>
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>

                            <dx:BootstrapLayoutItem Caption="Priority" ColSpanMd="6">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <dx:BootstrapComboBox ID="EditPriorityComboBox" runat="server"
                                            NullText="Select priority..."
                                            Width="100%">
                                            <Items>
                                                <dx:BootstrapListEditItem Text="Low" Value="Low" />
                                                <dx:BootstrapListEditItem Text="Medium" Value="Medium" />
                                                <dx:BootstrapListEditItem Text="High" Value="High" />
                                            </Items>
                                        </dx:BootstrapComboBox>
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>

                            <dx:BootstrapLayoutItem Caption="Attachment Name" ColSpanMd="6">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <dx:BootstrapTextBox ID="EditAttachmentTextBox" runat="server"
                                            NullText="e.g. document.pdf (optional)"
                                            Width="100%" />
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>

                            <dx:BootstrapLayoutItem ColSpanMd="12" ShowCaption="False">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <dx:BootstrapButton ID="EditGenerateAIButton" runat="server"
                                            Text="Generate AI Suggestions"
                                            SettingsBootstrap-RenderOption="Secondary"
                                            SettingsBootstrap-ButtonSize="Small"
                                            CausesValidation="false"
                                            AutoPostBack="true"
                                            OnClick="EditGenerateAIButton_Click" />
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>

                            <dx:BootstrapLayoutItem ColSpanMd="12" ShowCaption="False">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <asp:Panel ID="EditAISuggestionsPanel" runat="server" Visible="false">
                                            <div class="alert alert-info mb-0">
                                                <div class="fw-semibold mb-2">
                                                    AI Suggestions
                   
                                                </div>
                                                <div class="row">
                                                    <div class="col-md-12 mb-1">
                                                        <span class="text-muted">Summary:</span>
                                                        <asp:Literal ID="EditAiSummaryLiteral" runat="server" />
                                                    </div>
                                                    <div class="col-md-6 mb-1">
                                                        <span class="text-muted">Suggested Category:</span>
                                                        <asp:Literal ID="EditAiCategoryLiteral" runat="server" />
                                                    </div>
                                                    <div class="col-md-6">
                                                        <span class="text-muted">Suggested Priority:</span>
                                                        <asp:Literal ID="EditAiPriorityLiteral" runat="server" />
                                                    </div>
                                                </div>
                                            </div>
                                        </asp:Panel>
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>

                            <dx:BootstrapLayoutItem ColSpanMd="12" ShowCaption="False">
                                <ContentCollection>
                                    <dx:ContentControl runat="server">
                                        <asp:Panel ID="EditValidationPanel" runat="server"
                                            Visible="false"
                                            CssClass="alert alert-danger mb-0">
                                            <asp:Literal ID="EditValidationMessage" runat="server" />
                                        </asp:Panel>
                                    </dx:ContentControl>
                                </ContentCollection>
                            </dx:BootstrapLayoutItem>

                        </Items>
                    </dx:BootstrapFormLayout>

                    <div class="d-flex justify-content-end p-3 border-top">
                        <dx:BootstrapButton ID="SaveEditButton" runat="server"
                            Text="Save Changes"
                            SettingsBootstrap-RenderOption="Primary"
                            SettingsBootstrap-ButtonSize="Small"
                            OnClick="SaveEditButton_Click" />
                    </div>

                    <asp:HiddenField ID="EditAiSummaryHidden" runat="server" />
                    <asp:HiddenField ID="EditAiCategoryHidden" runat="server" />
                    <asp:HiddenField ID="EditAiPriorityHidden" runat="server" />
                </dx:ContentControl>
            </ContentCollection>
        </dx:BootstrapPopupControl>

        <!-- History -->
        <div class="mt-4">
            <h4 class="mb-3">Audit History</h4>
            <dx:BootstrapGridView ID="HistoryGridView" runat="server"
                KeyFieldName="Id"
                Width="100%">
                <Columns>
                    <dx:BootstrapGridViewTextColumn FieldName="Action" Caption="Action" />
                    <dx:BootstrapGridViewTextColumn FieldName="PerformedByName" Caption="Performed By" />
                    <dx:BootstrapGridViewTextColumn FieldName="NewStatus" Caption="Status" />
                    <dx:BootstrapGridViewDateColumn FieldName="PerformedAt" Caption="Date" />
                </Columns>
            </dx:BootstrapGridView>
        </div>

</asp:Content>
