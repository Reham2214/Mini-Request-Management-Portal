<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Mini_Request_Management_Portal._Default" %>

<%@ Register Assembly="DevExpress.Web.Bootstrap.v25.2, Version=25.2.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a"
    Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <!-- Page header -->
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h4 class="mb-0">All Requests</h4>
        <dx:BootstrapButton runat="server" Text="+ New Request" AutoPostBack="false" ID="CreateNewRequestButton" Visible="false">
            <SettingsBootstrap RenderOption="Primary" />
            <ClientSideEvents Click="function(s,e) { createNewRequestPopupControl.Show(); }" />
        </dx:BootstrapButton>
    </div>

    <!-- Create new request -->
    <dx:BootstrapPopupControl runat="server"
        ShowOnPageLoad="false"
        ID="CreateNewRequestPopupControl"
        ClientInstanceName="createNewRequestPopupControl"
        PopupHorizontalAlign="WindowCenter"
        PopupVerticalAlign="WindowCenter"
        Width="800px"
        CloseAction="CloseButton"
        HeaderText="New Request">
        <ContentCollection>
            <dx:ContentControl runat="server">

                <dx:BootstrapFormLayout ID="NewRequestFormLayout" runat="server">
                    <Items>

                        <dx:BootstrapLayoutItem Caption="Title" ColSpanMd="12">
                            <ContentCollection>
                                <dx:ContentControl runat="server">
                                    <dx:BootstrapTextBox ID="TitleTextBox" runat="server"
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
                                    <dx:BootstrapMemo ID="DescriptionMemo" runat="server"
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
                                    <dx:BootstrapComboBox ID="RequestTypeComboBox" runat="server"
                                        ClientInstanceName="requestTypeComboBox"
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
                                    <dx:BootstrapComboBox ID="DepartmentComboBox" runat="server"
                                        ClientInstanceName="departmentComboBox"
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
                                    <dx:BootstrapComboBox ID="PriorityComboBox" runat="server"
                                        Width="100%">
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Low" Value="Low" />
                                            <dx:BootstrapListEditItem Text="Medium" Value="Medium" Selected="true" />
                                            <dx:BootstrapListEditItem Text="High" Value="High" />
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </dx:ContentControl>
                            </ContentCollection>
                        </dx:BootstrapLayoutItem>

                        <dx:BootstrapLayoutItem Caption="Attachment Name" ColSpanMd="6">
                            <ContentCollection>
                                <dx:ContentControl runat="server">
                                    <dx:BootstrapTextBox ID="AttachmentTextBox" runat="server"
                                        NullText="e.g. document.pdf (optional)"
                                        Width="100%" />
                                </dx:ContentControl>
                            </ContentCollection>
                        </dx:BootstrapLayoutItem>

                        <dx:BootstrapLayoutItem ColSpanMd="12" ShowCaption="False">
                            <ContentCollection>
                                <dx:ContentControl runat="server">
                                    <dx:BootstrapButton ID="GenerateAISuggestionsButton" runat="server"
                                        Text="Generate AI Suggestions"
                                        SettingsBootstrap-RenderOption="Secondary"
                                        SettingsBootstrap-ButtonSize="Small"
                                        AutoPostBack="true"
                                        OnClick="GenerateAISuggestionsButton_Click" />
                                </dx:ContentControl>
                            </ContentCollection>
                        </dx:BootstrapLayoutItem>

                        <dx:BootstrapLayoutItem ColSpanMd="12" ShowCaption="False">
                            <ContentCollection>
                                <dx:ContentControl runat="server">
                                    <asp:Panel ID="AISuggestionsPanel" runat="server" Visible="false">
                                        <div class="alert alert-info mb-0">
                                            <div class="fw-semibold mb-2">
                                                AI Suggestions
                                            </div>
                                            <div class="row">
                                                <div class="col-md-12 mb-1">
                                                    <span class="text-muted">Summary:</span>
                                                    <asp:Literal ID="AiSummaryLiteral" runat="server" />
                                                </div>
                                                <div class="col-md-6 mb-1">
                                                    <span class="text-muted">Suggested Category:</span>
                                                    <asp:Literal ID="AiCategoryLiteral" runat="server" />
                                                </div>
                                                <div class="col-md-6">
                                                    <span class="text-muted">Suggested Priority:</span>
                                                    <asp:Literal ID="AiPriorityLiteral" runat="server" />
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
                                    <asp:Panel ID="ValidationPanel" runat="server"
                                        Visible="false"
                                        CssClass="alert alert-danger mb-0">
                                        <asp:Literal ID="ValidationMessage" runat="server" />
                                    </asp:Panel>
                                    <asp:Panel ID="SuccessPanel" runat="server"
                                        Visible="false"
                                        CssClass="alert alert-success mb-0">
                                        <asp:Literal ID="SuccessMessage" runat="server" />
                                    </asp:Panel>
                                </dx:ContentControl>
                            </ContentCollection>
                        </dx:BootstrapLayoutItem>

                    </Items>
                </dx:BootstrapFormLayout>

                <div class="d-flex justify-content-end gap-5 p-3 border-top">
                    <dx:BootstrapButton ID="SaveDraftButton" runat="server"
                        Text="Save as Draft"
                        SettingsBootstrap-RenderOption="Secondary"
                        SettingsBootstrap-ButtonSize="Small"
                        OnClick="SaveDraftButton_Click" />
                    
                    &nbsp;&nbsp;&nbsp;
               
                    <dx:BootstrapButton ID="SubmitRequestButton" runat="server"
                        Text="Submit Request"
                        SettingsBootstrap-RenderOption="Primary"
                        SettingsBootstrap-ButtonSize="Small"
                        OnClick="SubmitRequestButton_Click" />
                </div>
                <asp:HiddenField ID="AiSummaryHidden" runat="server" />
                <asp:HiddenField ID="AiCategoryHidden" runat="server" />
                <asp:HiddenField ID="AiPriorityHidden" runat="server" />
            </dx:ContentControl>
        </ContentCollection>
    </dx:BootstrapPopupControl>

    <!-- Requests list -->
    <dx:BootstrapGridView ID="gvRequests" runat="server"
        AutoGenerateColumns="false"
        KeyFieldName="Id">

        <SettingsPager PageSize="10" />
        <Settings ShowFilterRow="true" />
        <Settings ShowHeaderFilterButton="true" />

        <Columns>

            <dx:BootstrapGridViewDataColumn FieldName="Id"
                Caption="Id" Width="50px" />

            <dx:BootstrapGridViewDataColumn FieldName="Title"
                Caption="Title" Width="250px" />

            <dx:BootstrapGridViewDataColumn FieldName="RequestType"
                Caption="Request Type" Width="250px" />

            <dx:BootstrapGridViewDataColumn FieldName="Department"
                Caption="Department" Width="250px" />

            <dx:BootstrapGridViewDataColumn FieldName="Priority"
                Caption="Priority" Width="250px">
                <DataItemTemplate>
                    <%# Eval("Priority") %>
                </DataItemTemplate>
            </dx:BootstrapGridViewDataColumn>

            <dx:BootstrapGridViewDataColumn FieldName="Status"
                Caption="Status" Width="250px">
                <DataItemTemplate>
                    <%# Eval("Status") %>
                </DataItemTemplate>
            </dx:BootstrapGridViewDataColumn>

            <dx:BootstrapGridViewDataColumn FieldName="CreatedByName"
                Caption="Created By" Width="250px" />

            <dx:BootstrapGridViewDataColumn FieldName="CreatedAt"
                Caption="Created Date" Width="250px">
                <DataItemTemplate>
                    <%# Eval("CreatedAt") %>
                </DataItemTemplate>
            </dx:BootstrapGridViewDataColumn>

            <dx:BootstrapGridViewCommandColumn Caption="Action" Width="50px">
                <CustomButtons>
                    <dx:BootstrapGridViewCommandColumnCustomButton ID="buttonView" Text="View" />
                </CustomButtons>
            </dx:BootstrapGridViewCommandColumn>

        </Columns>
        <ClientSideEvents CustomButtonClick="onViewButtonClick" />
    </dx:BootstrapGridView>

</asp:Content>

<asp:Content ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        function onViewButtonClick(s, e) {
            if (e.buttonID === "buttonView") {
                var id = s.GetRowKey(e.visibleIndex);
                window.location.href = "Requests/Detail.aspx?id=" + id;
            }
        }
    </script>
</asp:Content>
