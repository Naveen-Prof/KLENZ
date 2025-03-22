--SELECT name FROM sys.databases;

--USE master;
--CREATE LOGIN [IIS APPPOOL\TestApp] FROM WINDOWS;
--ALTER SERVER ROLE sysadmin ADD MEMBER [IIS APPPOOL\TestApp];

--USE KLENZ1;
--CREATE USER [IIS APPPOOL\TestApp] FOR LOGIN [IIS APPPOOL\TestApp];
--ALTER ROLE db_owner ADD MEMBER [IIS APPPOOL\TestApp];


--CREATE SCHEMA Services

--CREATE SCHEMA Project

--CREATE SCHEMA Sales


IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'FinancialYear')
BEGIN
	CREATE TABLE Services.FinancialYear (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FyYear			NVARCHAR(250)   NOT NULL,
		IsActive		TINYINT			NOT NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_FinancialYear_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id)
	)
END
GO


IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'Companies')
BEGIN
	CREATE TABLE Services.Companies (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FullName		NVARCHAR(500)   NOT NULL,
		ShortName		NVARCHAR(250)   NOT NULL,
		IsActive		TINYINT			NOT NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_Companies_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id)
	)
END
GO

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
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'SalesEnquiry')
BEGIN
    CREATE TABLE Sales.SalesEnquiry (
        Id                INT             IDENTITY(1,1) PRIMARY KEY,
        CompanyNameId     INT			  NOT NULL,
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
        CONSTRAINT FK_SalesEnquiry_CompanyName FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id)
    )
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'QuotationReport')
BEGIN
	CREATE TABLE Sales.QuotationReport (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		QuotationDate	DATETIME		NULL,
		CompanyNameId   INT			    NOT NULL,
		ProductDetails	NVARCHAR(MAX)   NULL,
		CustomerDetails	NVARCHAR(MAX)   NULL,
		QuotationValue	DECIMAL(18,2)	NULL,
		Remarks			NVARCHAR(MAX)   NULL,
		IsPositive		TINYINT			NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_SalesQuotationReport_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
        CONSTRAINT FK_QuotationReport_CompanyName FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id)
	)
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'PositiveEnquiry')
BEGIN
	CREATE TABLE Sales.PositiveEnquiry (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		QuotationDate	DATETIME		NULL,
		CompanyNameId   INT			    NOT NULL,
		ProductDetails	NVARCHAR(MAX)   NULL,
		CustomerDetails	NVARCHAR(MAX)   NULL,
		QuotationValue	DECIMAL(18,2)	NULL,
		CurrentStatus	NVARCHAR(MAX)   NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_SalesPositiveEnquiry_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
		CONSTRAINT FK_PositiveEnquiry_CompanyName FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id)
	)
END
GO


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
GO


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
GO


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
GO


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
GO


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
GO