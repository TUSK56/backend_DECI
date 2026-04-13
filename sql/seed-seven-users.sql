/*
  Run AFTER migrations include [Users].[ProfileCompleted] and role columns.
  Inserts 1 Admin + 7 Coordinator accounts if those emails are not present (8 total).
  Login password for each: Deci123!
  Hash matches BCrypt.Net-Next (same as Deci.Api BcryptPasswordHasher).
  Seeded users have ProfileCompleted = 0 so the portal requires email, password, and phone on first login only.
*/
SET NOCOUNT ON;

DECLARE @hash nvarchar(256) = N'$2a$11$kMqd9kE.yAPwkjMIXk3KS.IH7WSpJJgOnLPmXETU1iSkbucmidOvO';
DECLARE @now datetime2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'admin@deci.local')
    INSERT INTO [Users] ([Email], [PasswordHash], [FullName], [Phone], [ProfileImagePath], [Role], [IsActive], [ProfileCompleted], [CreatedAt])
    VALUES (N'admin@deci.local', @hash, N'Administrator', N'', NULL, N'Admin', 1, 0, @now);

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'account1@deci.local')
    INSERT INTO [Users] ([Email], [PasswordHash], [FullName], [Phone], [ProfileImagePath], [Role], [IsActive], [ProfileCompleted], [CreatedAt])
    VALUES (N'account1@deci.local', @hash, N'Account One', N'', NULL, N'Coordinator', 1, 0, @now);

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'account2@deci.local')
    INSERT INTO [Users] ([Email], [PasswordHash], [FullName], [Phone], [ProfileImagePath], [Role], [IsActive], [ProfileCompleted], [CreatedAt])
    VALUES (N'account2@deci.local', @hash, N'Account Two', N'', NULL, N'Coordinator', 1, 0, @now);

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'account3@deci.local')
    INSERT INTO [Users] ([Email], [PasswordHash], [FullName], [Phone], [ProfileImagePath], [Role], [IsActive], [ProfileCompleted], [CreatedAt])
    VALUES (N'account3@deci.local', @hash, N'Account Three', N'', NULL, N'Coordinator', 1, 0, @now);

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'account4@deci.local')
    INSERT INTO [Users] ([Email], [PasswordHash], [FullName], [Phone], [ProfileImagePath], [Role], [IsActive], [ProfileCompleted], [CreatedAt])
    VALUES (N'account4@deci.local', @hash, N'Account Four', N'', NULL, N'Coordinator', 1, 0, @now);

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'account5@deci.local')
    INSERT INTO [Users] ([Email], [PasswordHash], [FullName], [Phone], [ProfileImagePath], [Role], [IsActive], [ProfileCompleted], [CreatedAt])
    VALUES (N'account5@deci.local', @hash, N'Account Five', N'', NULL, N'Coordinator', 1, 0, @now);

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'account6@deci.local')
    INSERT INTO [Users] ([Email], [PasswordHash], [FullName], [Phone], [ProfileImagePath], [Role], [IsActive], [ProfileCompleted], [CreatedAt])
    VALUES (N'account6@deci.local', @hash, N'Account Six', N'', NULL, N'Coordinator', 1, 0, @now);

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'account7@deci.local')
    INSERT INTO [Users] ([Email], [PasswordHash], [FullName], [Phone], [ProfileImagePath], [Role], [IsActive], [ProfileCompleted], [CreatedAt])
    VALUES (N'account7@deci.local', @hash, N'Account Seven', N'', NULL, N'Coordinator', 1, 0, @now);

/* Optional: singleton settings row (matches app defaults) */
IF NOT EXISTS (SELECT 1 FROM [SystemSettings])
    INSERT INTO [SystemSettings] ([ShiftStart], [ShiftEnd], [IpTrackingEnabled], [SessionApprovalRequired])
    VALUES ('09:00:00', '17:00:00', 1, 1);
