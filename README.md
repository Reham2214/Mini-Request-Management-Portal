# Mini Request Management Portal

## Project Overview
The Mini Request Management Portal was built as part of a required assessment for candidates in the MBC GDP program.
It is a web application that allows employees to create, edit, and track requests through a simple lifecycle. 
It supports two main roles: the Requester and the Reviewer. The Requester has the ability to create, edit, and cancel requests,
while the Reviewer can apply the appropriate next action based on the current status of the request.
An AI-powered feature has also been added to suggest a summary, category, and priority to the Requester based on the request content.
The goal was to build something that feels like a real internal tool. I focused on keeping the code clean, organized, and easy to follow.


## Technology Stack
- ASP.NET Web Forms (.NET Framework 4.7.2)
- C#
- SQLite — lightweight file-based database
- DevExpress Bootstrap v25.2 — UI components (grid, popup, form layout, card view)
- Bootstrap 4.6.2 — layout and styling
- Newtonsoft.Json — JSON serialization for the AI API call
- GitHub Models API (GPT-4o Mini) — AI suggestions feature

## Prerequisites
- Visual Studio 2022
- .NET Framework 4.7.2
- DevExpress 25.2

## How to Run the Project
1- Extract or clone the project folder
2- Open Mini Request Management Portal.sln in Visual Studio 2022
3- Right-click the solution in Solution Explorer and select Restore NuGet Packages
4- Build the solution using Build → Build Solution (Ctrl+Shift+B)
5- Press F5 to run the project or right click on the Default.aspx then select view in browser

## How the Database Works
The database is managed entirely by the application. When the project starts for the first time, 
DatabaseHelper.cs will:

1- Creates the .db file in the App_Data folder if it does not exist
2- Runs Schema.sql to create the tables, which are Users, Requests, RequestHistory

## User Roles and How to Switch
To switch between roles, click the Switch Role button in the top navigation bar. The button toggles between:

1- Requester — can create requests, edit drafts, submit, and cancel their own draft requests
2- Reviewer — can see all submitted requests, start reviews, complete requests, add reviewer comments, and cancel submitted

## Pages and Features

### Home Page (Request List)
- Displays all requests in a grid with search and filter support
- Requesters see only their own requests
- Reviewers see all requests except those cancelled directly from Draft by a requester
- Requesters can create a new request using the + New Request button

### Create New Request
- Opens as a popup form from the home page
- Required fields: Title, Description, Request Type, Department
- Optional fields: Priority, Attachment Name
- Includes an AI Suggestions button that generates a summary, suggested category, and suggested priority based on the title and description
- The user can save as a Draft or Submit directly

### Request Detail Page
- Displays all information for a selected request
- Shows the audit history
- Shows available workflow action buttons based on the current status and user role

### Edit Request
- Available only on Draft requests and only to the Requester who created it
- Opens as a popup form, and pre-filled with the current request data
- Includes the AI Suggestions feature so the user can regenerate suggestions after editing

## Workflow and Status Rules
Buttons only appear when the action is valid for the current status and role

- Draft → Submitted (Reviewer)
- Submitted → In Review (Requester)
- In Review → Completed (Requester)
- Draft → Cancelled (Reviewer)
- Submitted → Cancelled (Requester)

## Editability Rule
A request can only be edited by the Requester who created it while the request is in Draft status.

## AI Feature
The AI feature generates three things based on the request title and description:

- AI Summary
- AI Suggested Category
- AI Suggested Priority

The AI feature uses the GitHub Models API with the gpt-4o-mini model.
The GitHub token is stored directly in Default.aspx.cs and Detail.aspx.cs inside the CallGitHubModelsApi method.

To use your own token:

1- Go to https://github.com/settings/tokens
2- Generate a new classic token
3- Replace the token value in both files:

If the AI call fails for any reason, the application shows a clear message and continues working normally without AI suggestions.

## Demo

[▶️ Watch the project demo]
https://github.com/user-attachments/assets/d4f11823-404e-43dd-b77e-ad73f49c16be
