--  Mini Request Management Portal
--  Database Schema – SQLite
--  All tables are created only if they do not already exist so
--  this script is safe to run on every application start-up.


--  Users
--  Stores both Requesters and Reviewer/Admin accounts.
CREATE TABLE IF NOT EXISTS Users
(
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName    TEXT    NOT NULL,
    Role        TEXT    NOT NULL CHECK (Role IN ('Requester', 'Reviewer'))
);


--  Requests
CREATE TABLE IF NOT EXISTS Requests
(
    Id                   INTEGER PRIMARY KEY AUTOINCREMENT,
    Title                TEXT    NOT NULL,
    Description          TEXT,
    RequestType          TEXT,
    Department           TEXT,
    Priority             TEXT    NOT NULL DEFAULT 'Medium'
                             CHECK (Priority IN ('Low', 'Medium', 'High')),
    Status               TEXT    NOT NULL DEFAULT 'Draft'
                             CHECK (Status IN (
                                 'Draft',
                                 'Submitted',
                                 'In Review',
                                 'Completed',
                                 'Cancelled'
                             )),
    CreatedByUserId      INTEGER NOT NULL REFERENCES Users (Id),
    AttachmentName       TEXT,
    ReviewerComment      TEXT,
    AiSummary            TEXT,
    AiSuggestedCategory  TEXT,
    AiSuggestedPriority  TEXT,
    CreatedAt            TEXT    NOT NULL DEFAULT (datetime('now')),
    ModifiedAt           TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- Index used by the list view filters and search
CREATE INDEX IF NOT EXISTS IX_Requests_Status
    ON Requests (Status);

CREATE INDEX IF NOT EXISTS IX_Requests_CreatedByUserId
    ON Requests (CreatedByUserId);




--  RequestHistory
CREATE TABLE IF NOT EXISTS RequestHistory
(
    Id                INTEGER PRIMARY KEY AUTOINCREMENT,
    RequestId         INTEGER NOT NULL REFERENCES Requests (Id),
    Action            TEXT    NOT NULL,
    PerformedByUserId INTEGER NOT NULL REFERENCES Users (Id),
    PerformedByName   TEXT    NOT NULL,   -- denormalised for audit integrity
    OldStatus         TEXT,
    NewStatus         TEXT,
    Notes             TEXT,
    PerformedAt       TEXT    NOT NULL DEFAULT (datetime('now'))
);

-- Index used when loading history for a single request detail page
CREATE INDEX IF NOT EXISTS IX_RequestHistory_RequestId
    ON RequestHistory (RequestId);
