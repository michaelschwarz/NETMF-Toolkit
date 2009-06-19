USE SMTP
GO

--- sample data

DECLARE @UserID int
DECLARE @EmailID int
EXEC dbo.AddUser 'michael', 'password', @UserID OUTPUT
EXEC dbo.AddEmail @UserID, 'info', 'domain.local', 'Michael Schwarz', @EmailID OUTPUT
EXEC dbo.AddEmail @UserID, 'michael.schwarz', 'domain.local', 'Michael Schwarz', @EmailID OUTPUT
GO

DECLARE @UserID int
DECLARE @EmailID int
EXEC dbo.AddUser 'andre', 'password', @UserID OUTPUT
EXEC dbo.AddEmail @UserID, 'andre', 'domain.local', 'Andre Seifert', @EmailID OUTPUT
EXEC dbo.AddEmail @UserID, 'as', 'domain.local', 'Andre Seifert', @EmailID OUTPUT
GO



