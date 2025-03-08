IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'SalesEnquiry')
BEGIN
    CREATE TABLE Sales.SalesEnquiry (
        Id                INT             IDENTITY(1,1) PRIMARY KEY,
        CompanyName       NVARCHAR(MAX)   NOT NULL,
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

        CONSTRAINT FK_SalesEnquiry_CreatedUser FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id)
    )
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'QuotationReport')
BEGIN
	CREATE TABLE Sales.QuotationReport (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		QuotationDate	DATETIME		NULL,
		CompanyName		NVARCHAR(MAX)	NOT NULL,
		ProductDetails	NVARCHAR(MAX)   NULL,
		CustomerDetails	NVARCHAR(MAX)   NULL,
		QuotationValue	DECIMAL(18,2)	NULL,
		Remarks			NVARCHAR(MAX)   NULL,
		IsPositive		TINYINT			NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_SalesQuotationReport_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id)
	)
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'PositiveEnquiry')
BEGIN
	CREATE TABLE Sales.PositiveEnquiry (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		QuotationDate	DATETIME		NULL,
		CompanyName		NVARCHAR(MAX)	NOT NULL,
		ProductDetails	NVARCHAR(MAX)   NULL,
		CustomerDetails	NVARCHAR(MAX)   NULL,
		QuotationValue	DECIMAL(18,2)	NULL,
		CurrentStatus	NVARCHAR(MAX)   NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_SalesPositiveEnquiry_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id)
	)
END
GO

CREATE SCHEMA Klenz
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Klenz' AND TABLE_NAME = 'FinancialYear')
BEGIN
	CREATE TABLE Klenz.FinancialYear (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FyYear			NVARCHAR(250)   NOT NULL,
		IsActive		TINYINT			NOT NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_SalesPositiveEnquiry_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id)
	)
END
GO

IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Sales' AND TABLE_NAME = 'ProjectList')
BEGIN
	CREATE TABLE Sales.ProjectList (
		Id				INT				IDENTITY(1,1) PRIMARY KEY,
		FyYear			INT				NOT NULL,
		WorkOrderDate	DATETIME		NULL,
		CompanyName		NVARCHAR(600)	NULL,
		CustomerDetails NVARCHAR(MAX)   NULL,
		WorkDetails		NVARCHAR(MAX)   NULL,
		WorkOrderValue  DECIMAL(18,2)	NULL,
		Remarks         NVARCHAR(MAX)   NULL,
		CreatedDateTime DATETIME		NULL,
		CreatedUserId	NVARCHAR(450)	NULL,

		CONSTRAINT FK_SalesProjectList_CreatedUser	FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id),
		CONSTRAINT FK_SalesProjectList_FyYear   	FOREIGN KEY (FyYear)		REFERENCES Klenz.FinancialYear(Id)
	)
END
GO