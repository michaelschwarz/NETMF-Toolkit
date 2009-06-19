USE master
GO

IF EXISTS (SELECT * FROM sysdatabases WHERE name = 'SMTP')
BEGIN
	DROP DATABASE SMTP
END
GO

CREATE DATABASE SMTP
ON PRIMARY ( NAME=SMTP_Data, FILENAME='c:\temp\SMTP.mdf', SIZE=50MB, MAXSIZE=10000MB, FILEGROWTH=100MB )
LOG ON ( NAME=SMTP_Log, FILENAME='c:\temp\SMTP.ldf', SIZE=10MB, MAXSIZE=200MB, FILEGROWTH=10MB )
--- COLLATE SQL_Latin1_General_Cp1_CI_AS
GO

IF NOT EXISTS (SELECT * FROM sysdatabases WHERE name = 'SMTP')
BEGIN
	RAISERROR('Could not create database SMTP.', 14, 1)
END
GO

USE SMTP
GO

--- create tables

CREATE TABLE dbo.Users
(
	UserID int IDENTITY (1,1) NOT NULL,
	Username varchar(20) NOT NULL,
	Password varchar(20) NOT NULL,
	CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Username)
)
GO

CREATE TABLE dbo.Emails
(
	EmailID int IDENTITY (1,1) NOT NULL,
	UserID int,
	Email varchar(100),
	Domain varchar(100) NOT NULL,
	Realname nvarchar(200),
	
	CONSTRAINT PK_Emails PRIMARY KEY NONCLUSTERED (EmailID),
	CONSTRAINT U_EmailDomain UNIQUE CLUSTERED (Email, Domain)
)
GO

CREATE TABLE dbo.EmailSourceStorage
(
	EmailSourceStorageID int IDENTITY (1,1) NOT NULL,
	Source text NOT NULL,
	
	CONSTRAINT PK_EmailSourceStorage PRIMARY KEY CLUSTERED (EmailSourceStorageID)
)
GO

CREATE TABLE dbo.EmailStorage
(
	EmailStorageID int IDENTITY (1,1) NOT NULL,
	EmailSourceStorageID int NOT NULL,
	FromEmailID int NOT NULL,
	ToEmailID int NOT NULL,
	Subject nvarchar(200),
	SourceSize bigint,
	ReceivedDate datetime,
	
	CONSTRAINT PK_EmailStorage PRIMARY KEY NONCLUSTERED (EmailSourceStorageID),
	CONSTRAINT FK_EmailSourceStorage FOREIGN KEY (EmailSourceStorageID) REFERENCES dbo.EmailSourceStorage (EmailSourceStorageID) ON DELETE CASCADE
)
GO

CREATE CLUSTERED INDEX IDX_FromEmailID_ReceivedDate ON dbo.EmailStorage(FromEmailID, ReceivedDate)
GO

--- create procedures

CREATE PROCEDURE dbo.IsLocalMailbox
	(
		@Email varchar(100),
		@Domain varchar(100),
		@UserID int OUTPUT
	)
AS
	SELECT @UserID = UserID FROM dbo.Emails WHERE Email = @Email AND Domain = @Domain AND UserID IS NOT NULL
	
	IF @@ROWCOUNT = 0
	BEGIN
		SELECT @UserID = UserID FROM dbo.Emails WHERE Email = '*' AND Domain = @Domain AND UserID IS NOT NULL
	END
GO

CREATE PROCEDURE dbo.IsUser
	(
		@Username varchar(20),
		@Password varchar(20),
		@UserID int OUTPUT
	)
AS
	SELECT @UserID = UserID FROM dbo.Users WHERE Username = @Username AND Password = @Password
GO

CREATE PROCEDURE dbo.AddEmail
	(
		@UserID int,
		@Email varchar(120),
		@Domain varchar(120),
		@Realname nvarchar(200),
		@EmailID int OUTPUT
	)
AS
	INSERT INTO dbo.Emails (UserID, Email, Domain) VALUES (@UserID, @Email, @Domain)
	
	SET @EmailID = @@IDENTITY
GO

CREATE PROCEDURE dbo.AddUser
	(
		@Username varchar(20),
		@Password varchar(20),
		@UserID int OUTPUT
	)
AS
	INSERT INTO dbo.Users (Username, Password) VALUES (@Username, @Password)
	
	SET @UserID = @@IDENTITY
GO

CREATE PROCEDURE dbo.AddEmailToStorage
	(
		@ToEmailID int,
		@FromEmail varchar(100),
		@FromDomain varchar(100),
		@Subject nvarchar(200),
		@Source text,
		@Size bigint,
		@EmailStorageID int OUTPUT
	)
AS
	DECLARE @FromEmailID int
	SELECT @FromEmailID = EmailID FROM dbo.Emails WHERE Email = @FromEmail AND Domain = @FromDomain
		
	IF @FromEmailID IS NULL
	BEGIN
		EXEC dbo.AddEmail NULL, @FromEmail, @FromDomain, '', @FromEmailID OUTPUT
	END
	
	INSERT INTO dbo.EmailSourceStorage (Source) VALUES (@Source)
	
	INSERT INTO dbo.EmailStorage (EmailSourceStorageID, FromEmailID, ToEmailID, Subject, SourceSize, ReceivedDate)
		VALUES (@@IDENTITY, @FromEmailID, @ToEmailID, @Subject, @Size, GETDATE())
		
	SET @EmailStorageID = @@IDENTITY
GO

CREATE PROCEDURE dbo.GetEmails
	(
		@Username varchar(20),
		@Password varchar(20)
	)
AS
	SELECT A.EmailStorageID, A.EmailSourceStorageID, B.Email + '@' + B.Domain AS [To], D.Email + '@' + D.Domain AS [From], A.Subject, A.SourceSize, A.ReceivedDate FROM dbo.EmailStorage AS A INNER JOIN
		dbo.Emails AS B ON A.ToEmailID = B.EmailID INNER JOIN
		dbo.Users AS C ON B.UserID = C.UserID INNER JOIN
		dbo.Emails AS D ON A.FromEmailID = D.EmailID
	WHERE C.UserID IS NOT NULL AND C.Username = @Username AND C.Password = @Password
GO


-- ********** Update 1 **********

USE SMTP
GO

ALTER TABLE dbo.Emails ADD Status int DEFAULT (0)
GO

ALTER TABLE dbo.EmailStorage ADD Status int DEFAULT(0)
GO

ALTER TABLE dbo.EmailStorage ADD ExternalID uniqueidentifier
GO

ALTER TABLE dbo.Emails ADD ExternalID uniqueidentifier
GO


ALTER PROCEDURE dbo.AddEmailToStorage
	(
		@ToEmailID int,
		@FromEmail varchar(100),
		@FromDomain varchar(100),
		@Subject nvarchar(200),
		@Source text,
		@Size bigint,
		@Status int,
		@EmailStorageID int OUTPUT
	)
AS
	DECLARE @FromEmailID int
	SELECT @FromEmailID = EmailID FROM dbo.Emails WHERE Email = @FromEmail AND Domain = @FromDomain
		
	IF @FromEmailID IS NULL
	BEGIN
		EXEC dbo.AddEmail NULL, @FromEmail, @FromDomain, '', @FromEmailID OUTPUT
	END
	
	INSERT INTO dbo.EmailSourceStorage (Source) VALUES (@Source)
	
	INSERT INTO dbo.EmailStorage (EmailSourceStorageID, FromEmailID, ToEmailID, Subject, SourceSize, ReceivedDate, Status, ExternalID)
		VALUES (@@IDENTITY, @FromEmailID, @ToEmailID, @Subject, @Size, GETDATE(), @Status, NEWID())
		
	SET @EmailStorageID = @@IDENTITY
GO

ALTER PROCEDURE dbo.AddEmail
	(
		@UserID int,
		@Email varchar(120),
		@Domain varchar(120),
		@Realname nvarchar(200),
		@EmailID int OUTPUT
	)
AS
	INSERT INTO dbo.Emails (UserID, Email, Domain, Status, ExternalID) VALUES (@UserID, @Email, @Domain, 1, NEWID())
	
	SET @EmailID = @@IDENTITY
GO

-- ********** Update 2 **********

USE SMTP
GO

CREATE PROCEDURE dbo.IsLocalEmail
	(
		@Email varchar(100),
		@Domain varchar(100),
		@UserID int OUTPUT
	)
AS
	SELECT @UserID = UserID FROM dbo.Emails WHERE Email = @Email AND Domain = @Domain AND UserID IS NOT NULL
	
	IF @@ROWCOUNT = 0
	BEGIN
		SELECT @UserID = UserID FROM dbo.Emails WHERE Email = '*' AND Domain = @Domain AND UserID IS NOT NULL
	END
GO


ALTER PROCEDURE dbo.IsLocalMailbox
	(
		@Username varchar(20),
		@Password varchar(20),
		@UserID int OUTPUT
	)
AS
	SELECT @UserID = UserID FROM dbo.Users WHERE Username = @Username AND Password = @Password AND UserID IS NOT NULL
	
	IF @@ROWCOUNT = 0
	BEGIN
		SET @UserID = -1
	END
GO

CREATE PROCEDURE dbo.GetMessage
	(
		@EmailSourceStorageID int
	)
AS
	SELECT Source FROM dbo.EmailSourceStorage WHERE EmailSourceStorageID = @EmailSourceStorageID
GO

CREATE PROCEDURE dbo.DeleteEmail
	(
		@EmailStorageID int
	)
AS
	DELETE FROM dbo.EmailStorage WHERE EmailStorageID = @EmailStorageID
GO

ALTER PROCEDURE dbo.IsLocalEmail
	(
		@Email varchar(100),
		@Domain varchar(100),
		@UserID int OUTPUT
	)
AS
	SELECT @UserID = UserID FROM dbo.Emails WHERE Email = @Email AND Domain = @Domain AND UserID IS NOT NULL
	
	IF @@ROWCOUNT = 0
	BEGIN
		SELECT @UserID = UserID FROM dbo.Emails WHERE Email = NULL AND Domain = @Domain AND UserID IS NOT NULL
	END
GO

CREATE PROCEDURE dbo.GetEmailID
	(
		@Email varchar(100),
		@Domain varchar(100),
		@EmailID int OUTPUT
	)
AS
	SELECT @EmailID = EmailID FROM dbo.Emails WHERE Email = @Email AND Domain = @Domain AND UserID IS NOT NULL
	
	IF @@ROWCOUNT = 0
	BEGIN
		SELECT @EmailID = EmailID FROM dbo.Emails WHERE Email = '*' AND Domain = @Domain AND UserID IS NOT NULL
	END
GO



