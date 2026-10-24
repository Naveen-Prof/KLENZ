-- ==========================================================================================
-- DEPLOYMENT SCRIPT: Companies & Multi-Company Setup
-- Description: Run this script on the target SQL Server database after deployment.
-- It ensures schemas, tables, constraints, and the initial 4 companies are properly seeded:
--   1: KLENZ
--   2: ARCUS
--   3: ZION
--   4: GTN
-- It is idempotent: safe to execute multiple times on fresh or existing databases.
-- ==========================================================================================

PRINT '---------------------------------------------------------';
PRINT 'Starting KLENZ Multi-Company Deployment Script...';
PRINT '---------------------------------------------------------';

-- 1. Ensure Required Schemas Exist
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Services')
BEGIN
    EXEC('CREATE SCHEMA [Services]');
    PRINT 'Schema [Services] created.';
END

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Sales')
BEGIN
    EXEC('CREATE SCHEMA [Sales]');
    PRINT 'Schema [Sales] created.';
END

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Project')
BEGIN
    EXEC('CREATE SCHEMA [Project]');
    PRINT 'Schema [Project] created.';
END
GO

-- 2. Ensure Services.Companies Table Exists with SequenceNo
IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'Companies')
BEGIN
    CREATE TABLE Services.Companies (
        Id              INT             IDENTITY(1,1) PRIMARY KEY,
        SequenceNo      INT             NOT NULL CONSTRAINT DF_Companies_SequenceNo DEFAULT 1,
        FullName        NVARCHAR(500)   NOT NULL,
        ShortName       NVARCHAR(250)   NOT NULL,
        IsActive        TINYINT         NOT NULL CONSTRAINT DF_Companies_IsActive DEFAULT 1,
        CreatedDateTime DATETIME        NULL CONSTRAINT DF_Companies_CreatedDateTime DEFAULT GETDATE(),
        CreatedUserId   NVARCHAR(450)   NULL,

        CONSTRAINT FK_Companies_CreatedUser FOREIGN KEY (CreatedUserId) REFERENCES dbo.AspNetUsers(Id)
    );
    PRINT 'Table Services.Companies created.';
END
ELSE
BEGIN
    -- Ensure SequenceNo column exists if table was created in an older schema
    IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'Companies' AND COLUMN_NAME = 'SequenceNo')
    BEGIN
        ALTER TABLE Services.Companies ADD SequenceNo INT NULL;
        EXEC('UPDATE Services.Companies SET SequenceNo = Id WHERE SequenceNo IS NULL');
        ALTER TABLE Services.Companies ALTER COLUMN SequenceNo INT NOT NULL;
        ALTER TABLE Services.Companies ADD CONSTRAINT DF_Companies_SequenceNo DEFAULT 1 FOR SequenceNo;
        PRINT 'Added SequenceNo column to Services.Companies.';
    END
END
GO

-- 3. Seed / Synchronize Initial Companies (KLENZ, ARCUS, ZION, GTN)
-- If table is completely empty, insert with explicit IDs 1, 2, 3, 4
IF NOT EXISTS (SELECT TOP 1 1 FROM Services.Companies)
BEGIN
    SET IDENTITY_INSERT Services.Companies ON;

    INSERT INTO Services.Companies (Id, SequenceNo, FullName, ShortName, IsActive, CreatedDateTime)
    VALUES 
        (1, 1, 'KLENZ', 'KLENZ', 1, GETDATE()),
        (2, 2, 'ARCUS', 'ARCUS', 1, GETDATE()),
        (3, 3, 'ZION ENTERPRICES', 'ZION', 1, GETDATE()),
        (4, 4, 'GTN', 'GTN', 1, GETDATE());

    SET IDENTITY_INSERT Services.Companies OFF;
    PRINT 'Seeded fresh companies with IDs 1 to 4.';
END
ELSE
BEGIN
    -- Table already has rows: Upsert each company and align sequence numbers
    -- 1: KLENZ
    IF NOT EXISTS (SELECT 1 FROM Services.Companies WHERE ShortName = 'KLENZ')
    BEGIN
        INSERT INTO Services.Companies (SequenceNo, FullName, ShortName, IsActive, CreatedDateTime)
        VALUES (1, 'KLENZ', 'KLENZ', 1, GETDATE());
        PRINT 'Inserted company: KLENZ';
    END
    ELSE
    BEGIN
        UPDATE Services.Companies SET SequenceNo = 1, IsActive = 1 WHERE ShortName = 'KLENZ';
    END

    -- 2: ARCUS
    IF NOT EXISTS (SELECT 1 FROM Services.Companies WHERE ShortName = 'ARCUS')
    BEGIN
        INSERT INTO Services.Companies (SequenceNo, FullName, ShortName, IsActive, CreatedDateTime)
        VALUES (2, 'ARCUS', 'ARCUS', 1, GETDATE());
        PRINT 'Inserted company: ARCUS';
    END
    ELSE
    BEGIN
        UPDATE Services.Companies SET SequenceNo = 2, IsActive = 1 WHERE ShortName = 'ARCUS';
    END

    -- 3: ZION
    IF NOT EXISTS (SELECT 1 FROM Services.Companies WHERE ShortName = 'ZION' OR FullName LIKE '%ZION%')
    BEGIN
        INSERT INTO Services.Companies (SequenceNo, FullName, ShortName, IsActive, CreatedDateTime)
        VALUES (3, 'ZION ENTERPRICES', 'ZION', 1, GETDATE());
        PRINT 'Inserted company: ZION';
    END
    ELSE
    BEGIN
        UPDATE Services.Companies SET SequenceNo = 3, IsActive = 1 WHERE ShortName = 'ZION' OR FullName LIKE '%ZION%';
    END

    -- 4: GTN
    IF NOT EXISTS (SELECT 1 FROM Services.Companies WHERE ShortName = 'GTN' OR FullName LIKE '%GTN%')
    BEGIN
        INSERT INTO Services.Companies (SequenceNo, FullName, ShortName, IsActive, CreatedDateTime)
        VALUES (4, 'GTN', 'GTN', 1, GETDATE());
        PRINT 'Inserted company: GTN';
    END
    ELSE
    BEGIN
        UPDATE Services.Companies SET SequenceNo = 4, IsActive = 1 WHERE ShortName = 'GTN' OR FullName LIKE '%GTN%';
    END
END
GO

-- 4. Ensure FinancialYear has CompanyNameId and Foreign Key
IF EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'FinancialYear')
BEGIN
    IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'FinancialYear' AND COLUMN_NAME = 'CompanyNameId')
    BEGIN
        ALTER TABLE Services.FinancialYear ADD CompanyNameId INT NULL;
        ALTER TABLE Services.FinancialYear ADD CONSTRAINT FK_FinancialYear_Company FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id);
        PRINT 'Added CompanyNameId to Services.FinancialYear.';
    END

    -- Backfill orphaned FinancialYear records to Company 1 (KLENZ)
    DECLARE @DefaultCompanyId INT;
    SELECT TOP 1 @DefaultCompanyId = Id FROM Services.Companies WHERE SequenceNo = 1;
    IF @DefaultCompanyId IS NULL
        SELECT TOP 1 @DefaultCompanyId = Id FROM Services.Companies ORDER BY SequenceNo;

    IF @DefaultCompanyId IS NOT NULL
    BEGIN
        EXEC('UPDATE Services.FinancialYear SET CompanyNameId = ' + @DefaultCompanyId + ' WHERE CompanyNameId IS NULL');
        PRINT 'Linked unassigned FinancialYear records to default company ID ' + CAST(@DefaultCompanyId AS NVARCHAR(10)) + '.';
    END
END
GO

-- 5. Verification Query
PRINT '---------------------------------------------------------';
PRINT 'Companies Table State:';
PRINT '---------------------------------------------------------';
SELECT 
    Id AS [CompanyId],
    SequenceNo AS [Seq],
    ShortName AS [Code],
    FullName AS [Company Full Name],
    CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS [Status],
    CreatedDateTime
FROM Services.Companies
ORDER BY SequenceNo ASC;
GO
PRINT '---------------------------------------------------------';
PRINT 'Deployment script completed successfully!';
PRINT '---------------------------------------------------------';
GO
