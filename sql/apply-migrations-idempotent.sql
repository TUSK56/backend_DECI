/*
  SCHEMA ONLY — this script does NOT insert users or any app data.
  After it succeeds, run seed-seven-users.sql OR start the Deci.Api once (DbSeeder) to create the 8 seeded accounts (1 Admin + 7 Coordinators).

  Migration id must be exactly: 20260412184748_InitialCreate (not 202404...).
*/
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [MasterGroupCodes] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(128) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_MasterGroupCodes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [MasterTrainers] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(256) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_MasterTrainers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(256) NOT NULL,
        [Body] nvarchar(2000) NOT NULL,
        [ForUserId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Read] bit NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [SystemSettings] (
        [Id] int NOT NULL IDENTITY,
        [ShiftStart] time NOT NULL,
        [ShiftEnd] time NOT NULL,
        [IpTrackingEnabled] bit NOT NULL,
        [SessionApprovalRequired] bit NOT NULL,
        CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Email] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(256) NOT NULL,
        [FullName] nvarchar(256) NOT NULL,
        [Phone] nvarchar(64) NULL,
        [ProfileImagePath] nvarchar(512) NULL,
        [Role] nvarchar(32) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [AttendanceRecords] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClockIn] datetime2 NOT NULL,
        [ClockOut] datetime2 NULL,
        [Ip] nvarchar(64) NULL,
        [DateLabel] nvarchar(64) NOT NULL,
        CONSTRAINT [PK_AttendanceRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AttendanceRecords_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [ChatMessages] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Text] nvarchar(4000) NOT NULL,
        [SentAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatMessages_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [LeaveRequests] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Type] nvarchar(64) NOT NULL,
        [Start] nvarchar(32) NOT NULL,
        [End] nvarchar(32) NOT NULL,
        [Reason] nvarchar(2000) NOT NULL,
        [Status] nvarchar(32) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LeaveRequests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [SessionLogs] (
        [Id] int NOT NULL IDENTITY,
        [CoordinatorUserId] int NOT NULL,
        [GroupCode] nvarchar(128) NOT NULL,
        [TrainerName] nvarchar(256) NOT NULL,
        [SessionDate] nvarchar(32) NOT NULL,
        [SessionLink] nvarchar(2048) NULL,
        [RecordingLink] nvarchar(2048) NULL,
        [Notes] nvarchar(4000) NULL,
        [Status] nvarchar(32) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SessionLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SessionLogs_Users_CoordinatorUserId] FOREIGN KEY ([CoordinatorUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [Tasks] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(512) NOT NULL,
        [Description] nvarchar(4000) NULL,
        [AssigneeId] int NOT NULL,
        [CreatedById] int NOT NULL,
        [Priority] nvarchar(32) NOT NULL,
        [Deadline] nvarchar(32) NULL,
        [Status] nvarchar(32) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Tasks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Tasks_Users_AssigneeId] FOREIGN KEY ([AssigneeId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Tasks_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE TABLE [SessionFiles] (
        [Id] int NOT NULL IDENTITY,
        [SessionLogId] int NOT NULL,
        [Kind] nvarchar(32) NOT NULL,
        [StoredPath] nvarchar(512) NOT NULL,
        [OriginalName] nvarchar(256) NOT NULL,
        CONSTRAINT [PK_SessionFiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SessionFiles_SessionLogs_SessionLogId] FOREIGN KEY ([SessionLogId]) REFERENCES [SessionLogs] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AttendanceRecords_UserId] ON [AttendanceRecords] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_UserId] ON [ChatMessages] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_UserId] ON [LeaveRequests] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MasterGroupCodes_Code] ON [MasterGroupCodes] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SessionFiles_SessionLogId] ON [SessionFiles] ([SessionLogId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SessionLogs_CoordinatorUserId] ON [SessionLogs] ([CoordinatorUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tasks_AssigneeId] ON [Tasks] ([AssigneeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tasks_CreatedById] ON [Tasks] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260412184748_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260412184748_InitialCreate', N'8.0.11');
END;
GO

