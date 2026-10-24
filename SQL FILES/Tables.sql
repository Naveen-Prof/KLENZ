--SELECT name FROM sys.databases;

--USE master;
--CREATE LOGIN [IIS APPPOOL\TestApp] FROM WINDOWS;
--ALTER SERVER ROLE sysadmin ADD MEMBER [IIS APPPOOL\TestApp];

--USE KLENZ1;
--CREATE USER [IIS APPPOOL\TestApp] FOR LOGIN [IIS APPPOOL\TestApp];
--ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\TestApp];


IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Services')
BEGIN
    EXEC('CREATE SCHEMA [Services]')
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Project')
BEGIN
    EXEC('CREATE SCHEMA [Project]')
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Sales')
BEGIN
    EXEC('CREATE SCHEMA [Sales]')
END

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'Companies')
BEGIN
	CREATE TABLE Services.Companies (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		SequenceNo		INT				NOT NULL CONSTRAINT DF_Companies_SequenceNo DEFAULT 1,
		FullName		NVARCHAR(500)   NOT NULL,
		ShortName		NVARCHAR(250)   NOT NULL,
		IsActive		TINYINT			NOT NULL CONSTRAINT DF_Companies_IsActive DEFAULT 1,
		CreatedDateTime DATETIME		NULL CONSTRAINT DF_Companies_CreatedDateTime DEFAULT GETDATE(),
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_Companies_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id)
	)

	-- Seed initial companies
	SET IDENTITY_INSERT Services.Companies ON;
	INSERT INTO Services.Companies (Id, SequenceNo, FullName, ShortName, IsActive, CreatedDateTime)
	VALUES 
		(1, 1, 'KLENZ', 'KLENZ', 1, GETDATE()),
		(2, 2, 'ARCUS', 'ARCUS', 1, GETDATE()),
		(3, 3, 'ZION ENTERPRICES', 'ZION', 1, GETDATE()),
		(4, 4, 'GTN', 'GTN', 1, GETDATE());
	SET IDENTITY_INSERT Services.Companies OFF;
END
ELSE
BEGIN
	IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'Companies' AND COLUMN_NAME = 'SequenceNo')
	BEGIN
		ALTER TABLE Services.Companies ADD SequenceNo INT NULL;
		EXEC('UPDATE Services.Companies SET SequenceNo = Id WHERE SequenceNo IS NULL');
		ALTER TABLE Services.Companies ALTER COLUMN SequenceNo INT NOT NULL;
		ALTER TABLE Services.Companies ADD CONSTRAINT DF_Companies_SequenceNo DEFAULT 1 FOR SequenceNo;
	END
END


IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'FinancialYear')
BEGIN
	CREATE TABLE Services.FinancialYear (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		StartDate       DATETIME        NOT NULL,
		EndDate         DATETIME        NOT NULL,
		FyYear			NVARCHAR(250)   NOT NULL,
		IsActive		TINYINT			NOT NULL,
		CompanyNameId	INT				NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_FinancialYear_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
		CONSTRAINT FK_FinancialYear_Company		FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id)
	)
END
ELSE
BEGIN
	IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'FinancialYear' AND COLUMN_NAME = 'StartDate')
	BEGIN
		ALTER TABLE Services.FinancialYear ADD StartDate DATETIME NOT NULL CONSTRAINT DF_FinancialYear_StartDate DEFAULT '2025-04-01';
	END
	IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'FinancialYear' AND COLUMN_NAME = 'EndDate')
	BEGIN
		ALTER TABLE Services.FinancialYear ADD EndDate DATETIME NOT NULL CONSTRAINT DF_FinancialYear_EndDate DEFAULT '2026-03-31';
	END
	IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'FinancialYear' AND COLUMN_NAME = 'CompanyNameId')
	BEGIN
		ALTER TABLE Services.FinancialYear ADD CompanyNameId INT NULL;
		ALTER TABLE Services.FinancialYear ADD CONSTRAINT FK_FinancialYear_Company FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id);
	END
END


IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'GSTTypes')
BEGIN
	CREATE TABLE Services.GSTTypes (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		GSTType 		INT             NOT NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_GSTTypes_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id)
	)
END


IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'Status')
BEGIN
	CREATE TABLE Sales.Status (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FullName		NVARCHAR(500)   NOT NULL,
		ShortName		NVARCHAR(250)   NOT NULL
	)
END


IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'SalesEnquiry')
BEGIN
    CREATE TABLE Sales.SalesEnquiry (
        Id                INT             IDENTITY(1,1) PRIMARY KEY,
        CompanyNameId     INT			  NOT NULL,
		FyYear			  INT			  NOT NULL,
        ReferedBy         NVARCHAR(MAX)   NULL,
        EnquiryDetails    NVARCHAR(MAX)   NULL,
        EnquiryDate       DATETIME        NULL,
        CustomerDetails   NVARCHAR(MAX)   NULL,
        Status            NVARCHAR(MAX)   NULL,
        Remarks           NVARCHAR(MAX)   NULL,
        ReminderDate      DATETIME        NULL,
        ReminderPlace     NVARCHAR(MAX)   NULL,
        FilePath          NVARCHAR(MAX)   NULL,
        CreatedDateTime   DATETIME        NULL,
        CreatedUserId     NVARCHAR(450)	  NULL, 
		
        CONSTRAINT FK_SalesEnquiry_CreatedUser FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_SalesEnquiry_CompanyName FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id),
		CONSTRAINT FK_SalesEnquiry_FyYear	   FOREIGN KEY (FyYear)		   REFERENCES Services.FinancialYear(Id)
    )
END


IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'QuotationReport')
BEGIN
	CREATE TABLE Sales.QuotationReport (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		QuotationDate	DATETIME		NULL,
		CompanyNameId   INT			    NOT NULL,
		FyYear			INT				NOT NULL,
		ProductDetails	NVARCHAR(MAX)   NULL,
		CustomerDetails	NVARCHAR(MAX)   NULL,
		QuotationValue	DECIMAL(18,2)	NULL,
		Remarks			NVARCHAR(MAX)   NULL,
		IsPositive		TINYINT			NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_SalesQuotationReport_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_SalesQuotationReport_CompanyName	FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id),
		CONSTRAINT FK_SalesQuotationReport_FyYear   	FOREIGN KEY (FyYear)		REFERENCES Services.FinancialYear(Id)
	)
END


IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'PositiveEnquiry')
BEGIN
	CREATE TABLE Sales.PositiveEnquiry (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		QuotationDate	DATETIME		NULL,
		CompanyNameId   INT			    NOT NULL,
		FyYear			INT				NOT NULL,
		ProductDetails	NVARCHAR(MAX)   NULL,
		CustomerDetails	NVARCHAR(MAX)   NULL,
		QuotationValue	DECIMAL(18,2)	NULL,
		CurrentStatus	NVARCHAR(MAX)   NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_SalesPositiveEnquiry_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
		CONSTRAINT FK_SalesPositiveEnquiry_CompanyName	FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id),
		CONSTRAINT FK_SalesPositiveEnquiry_FyYear   	FOREIGN KEY (FyYear)		REFERENCES Services.FinancialYear(Id)
	)
END



IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'ProjectList')
BEGIN
	CREATE TABLE Sales.ProjectList (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FyYear			INT				NOT NULL,
		WorkOrderDate	DATETIME		NULL,
		CompanyNameId   INT			    NOT NULL,
		CustomerDetails NVARCHAR(MAX)   NULL,
		WorkDetails		NVARCHAR(MAX)   NULL,
		WorkOrderValue  DECIMAL(18,2)	NULL,
		Remarks         NVARCHAR(MAX)   NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_SalesProjectList_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
		CONSTRAINT FK_SalesProjectList_FyYear   	FOREIGN KEY (FyYear)		REFERENCES Services.FinancialYear(Id),
		CONSTRAINT FK_ProjectList_CompanyName       FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id)

	)
END



IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Project' AND TABLE_NAME = 'Consultancy')
BEGIN
	CREATE TABLE Project.Consultancy (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FyYear			INT				NOT NULL,
		WorkOrderDate	DATETIME		NULL,
		CompanyNameId   INT			    NOT NULL,
		CustomerDetails NVARCHAR(MAX)   NULL,
		WorkDetails		NVARCHAR(MAX)   NULL,
		ProjectCost     DECIMAL(18,2)	NULL,
		GSTTypeId		INT             NULL, --Computed column
		Total			DECIMAL(18,2)	NULL, --Computed column
		WorkDuration	NVARCHAR(100)	NULL,
		Remarks         NVARCHAR(MAX)   NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_Project_Consultancy_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
		CONSTRAINT FK_Project_Consultancy_FyYear   	    FOREIGN KEY (FyYear)		REFERENCES Services.FinancialYear(Id),
		CONSTRAINT FK_Project_Consultancy_CompanyName   FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id),
		CONSTRAINT FK_Project_Consultancy_GSTType       FOREIGN KEY (GSTTypeId)		REFERENCES Services.GSTTypes(Id)
	)
END



IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Project' AND TABLE_NAME = 'TenderList')
BEGIN
	CREATE TABLE Project.TenderList (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FyYear			INT				NOT NULL,
		WorkOrderDate	DATETIME		NULL,
		CompanyNameId   INT			    NOT NULL,
		CustomerDetails NVARCHAR(MAX)   NULL,
		WorkDetails		NVARCHAR(MAX)   NULL,
		ProjectCost     DECIMAL(18,2)	NULL,
		GSTTypeId		INT             NULL,--Computed column
		Total			DECIMAL(18,2)	NULL, --Computed column
		WorkDuration	NVARCHAR(100)	NULL,
		Remarks         NVARCHAR(MAX)   NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_Project_TenderList_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
		CONSTRAINT FK_Project_TenderList_FyYear   	    FOREIGN KEY (FyYear)		REFERENCES Services.FinancialYear(Id),
		CONSTRAINT FK_Project_TenderList_Consultancy    FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id),
		CONSTRAINT FK_Project_TenderList_GSTType        FOREIGN KEY (GSTTypeId)		REFERENCES Services.GSTTypes(Id)
	)
END



IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Project' AND TABLE_NAME = 'ToBeTenderList')
BEGIN
	CREATE TABLE Project.ToBeTenderList (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FyYear			INT				NOT NULL,
		WorkOrderDate	DATETIME		NULL,
		CompanyNameId   INT			    NOT NULL,
		CustomerDetails NVARCHAR(MAX)   NULL,
		WorkDetails		NVARCHAR(MAX)   NULL,
		ProjectCost     DECIMAL(18,2)	NULL,
		GSTTypeId		INT             NULL, --Computed column
		Total			DECIMAL(18,2)	NULL, --Computed column
		WorkDuration	NVARCHAR(100)	NULL,
		Remarks         NVARCHAR(MAX)   NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_Project_ToBeTenderList_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
		CONSTRAINT FK_Project_ToBeTenderList_FyYear   	    FOREIGN KEY (FyYear)		REFERENCES Services.FinancialYear(Id),
		CONSTRAINT FK_Project_ToBeTenderList_Consultancy    FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id),
		CONSTRAINT FK_Project_ToBeTenderList_GSTType        FOREIGN KEY (GSTTypeId)		REFERENCES Services.GSTTypes(Id)
	)
END



IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Project' AND TABLE_NAME = 'KlenzChemicals')
BEGIN
	CREATE TABLE Project.KlenzChemicals (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FyYear			INT				NOT NULL,
		WorkOrderDate	DATETIME		NULL,
		CompanyNameId   INT			    NOT NULL,
		CustomerDetails NVARCHAR(MAX)   NULL,
		WorkDetails		NVARCHAR(MAX)   NULL,
		ProjectCost     DECIMAL(18,2)	NULL,
		GSTTypeId		INT             NULL, --Computed column
		Total			DECIMAL(18,2)	NULL, --Computed column
		WorkDuration	NVARCHAR(100)	NULL,
		Remarks         NVARCHAR(MAX)   NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_Project_KlenzChemicals_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
		CONSTRAINT FK_Project_KlenzChemicals_FyYear   	    FOREIGN KEY (FyYear)		REFERENCES Services.FinancialYear(Id),
		CONSTRAINT FK_Project_KlenzChemicals_Consultancy    FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id),
		CONSTRAINT FK_Project_KlenzChemicals_GSTType        FOREIGN KEY (GSTTypeId)		REFERENCES Services.GSTTypes(Id)
	)
END
