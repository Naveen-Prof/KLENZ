-- =======================================================================
-- Query: Create ARCUS and update sequence numbers for all companies
-- Sequence order:
-- 1: KLENZ
-- 2: ARCUS
-- 3: ZION
-- 4: GTN
-- =======================================================================

-- 1. Create ARCUS company if not already created
IF NOT EXISTS (SELECT 1 FROM Services.Companies WHERE ShortName = 'ARCUS')
BEGIN
    INSERT INTO Services.Companies (SequenceNo, FullName, ShortName, IsActive, CreatedDateTime)
    VALUES (2, 'ARCUS', 'ARCUS', 1, GETDATE());
END
GO

-- 2. Update sequence numbers
UPDATE Services.Companies SET SequenceNo = 1 WHERE ShortName = 'KLENZ';
UPDATE Services.Companies SET SequenceNo = 2 WHERE ShortName = 'ARCUS';
UPDATE Services.Companies SET SequenceNo = 3 WHERE ShortName = 'ZION' OR FullName LIKE '%ZION%';
UPDATE Services.Companies SET SequenceNo = 4 WHERE ShortName = 'GTN' OR FullName LIKE '%GTN%';
GO

-- 3. Verification query
SELECT SequenceNo, ShortName, FullName, IsActive, CreatedDateTime
FROM Services.Companies
ORDER BY SequenceNo ASC;
GO
