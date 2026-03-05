-- =============================================================================
-- File   : TodoItems.sql
-- Purpose: DDL and DML scripts for the ToDoItems table.
--          The application currently uses in-memory storage; these scripts
--          are provided for future database persistence.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. Create the database (run once, adjust name as needed)
-- -----------------------------------------------------------------------------
-- CREATE DATABASE ToDoDb;
-- GO
-- USE ToDoDb;
-- GO

-- -----------------------------------------------------------------------------
-- 2. Create the TodoItems table
-- -----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM   sys.tables
    WHERE  name   = 'TodoItems'
    AND    schema_id = SCHEMA_ID('dbo')
)
BEGIN
    CREATE TABLE dbo.TodoItems
    (
        Id           UNIQUEIDENTIFIER  NOT NULL  DEFAULT NEWSEQUENTIALID()  CONSTRAINT PK_TodoItems PRIMARY KEY,
        Title        NVARCHAR(255)     NOT NULL,
        Description  NVARCHAR(MAX)         NULL,
        IsCompleted  BIT               NOT NULL  DEFAULT 0,
        CreatedAtUtc DATETIME2(7)      NOT NULL  DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc DATETIME2(7)      NOT NULL  DEFAULT SYSUTCDATETIME()
    );

    PRINT 'Table dbo.TodoItems created.';
END
ELSE
BEGIN
    PRINT 'Table dbo.TodoItems already exists - skipped.';
END
GO

-- -----------------------------------------------------------------------------
-- 3. Index: speed up status-based filtering
-- -----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM   sys.indexes
    WHERE  name   = 'IX_TodoItems_IsCompleted'
    AND    object_id = OBJECT_ID('dbo.TodoItems')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TodoItems_IsCompleted
        ON dbo.TodoItems (IsCompleted)
        INCLUDE (Title, Description, CreatedAtUtc, UpdatedAtUtc);

    PRINT 'Index IX_TodoItems_IsCompleted created.';
END
GO

-- Index: speed up date-based sorting
IF NOT EXISTS (
    SELECT 1
    FROM   sys.indexes
    WHERE  name   = 'IX_TodoItems_CreatedAtUtc'
    AND    object_id = OBJECT_ID('dbo.TodoItems')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TodoItems_CreatedAtUtc
        ON dbo.TodoItems (CreatedAtUtc DESC);

    PRINT 'Index IX_TodoItems_CreatedAtUtc created.';
END
GO

-- =============================================================================
-- CRUD Stored Procedures
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 4. GetAllTodoItems  - filtered + sorted list
--
-- FIX (B-01 / HIGH): The previous ORDER BY pattern used a mixed-direction CASE
-- expression on a single ORDER BY clause, which is fragile and raises a parse
-- error on SQL Server 2012 and earlier.  The correct idiomatic SQL Server
-- pattern is to use two separate CASE expressions - one ASC and one DESC.
-- Only the active branch returns a non-NULL value; the inactive branch returns
-- NULL, which sorts as lowest and therefore has no effect on the result set.
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.GetAllTodoItems
    @Status    NVARCHAR(20) = 'All',        -- 'All' | 'Active' | 'Completed'
    @SortOrder NVARCHAR(20) = 'NewestFirst' -- 'NewestFirst' | 'OldestFirst'
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Title,
        Description,
        IsCompleted,
        CreatedAtUtc,
        UpdatedAtUtc
    FROM dbo.TodoItems
    WHERE
        (@Status = 'All')
        OR (@Status = 'Active'     AND IsCompleted = 0)
        OR (@Status = 'Completed'  AND IsCompleted = 1)
    ORDER BY
        -- ASC branch: only active when @SortOrder = 'OldestFirst';
        -- returns NULL (sorts lowest) for all other values, so it has no effect.
        CASE WHEN @SortOrder = 'OldestFirst' THEN CreatedAtUtc END ASC,
        -- DESC branch: only active when @SortOrder = 'NewestFirst';
        -- returns NULL (sorts lowest) for all other values, so it has no effect.
        CASE WHEN @SortOrder = 'NewestFirst' THEN CreatedAtUtc END DESC;
END
GO

-- -----------------------------------------------------------------------------
-- 5. GetTodoItemById
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.GetTodoItemById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Title,
        Description,
        IsCompleted,
        CreatedAtUtc,
        UpdatedAtUtc
    FROM dbo.TodoItems
    WHERE Id = @Id;
END
GO

-- -----------------------------------------------------------------------------
-- 6. CreateTodoItem
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.CreateTodoItem
    @Id          UNIQUEIDENTIFIER,
    @Title       NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TodoItems (Id, Title, Description, IsCompleted, CreatedAtUtc, UpdatedAtUtc)
    VALUES (@Id, @Title, @Description, 0, SYSUTCDATETIME(), SYSUTCDATETIME());

    SELECT
        Id,
        Title,
        Description,
        IsCompleted,
        CreatedAtUtc,
        UpdatedAtUtc
    FROM dbo.TodoItems
    WHERE Id = @Id;
END
GO

-- -----------------------------------------------------------------------------
-- 7. UpdateTodoItem  - full update (title, description, isCompleted)
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.UpdateTodoItem
    @Id          UNIQUEIDENTIFIER,
    @Title       NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL,
    @IsCompleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TodoItems
    SET
        Title        = @Title,
        Description  = @Description,
        IsCompleted  = @IsCompleted,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE Id = @Id;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS Success;
        RETURN;
    END

    SELECT
        Id,
        Title,
        Description,
        IsCompleted,
        CreatedAtUtc,
        UpdatedAtUtc
    FROM dbo.TodoItems
    WHERE Id = @Id;
END
GO

-- -----------------------------------------------------------------------------
-- 8. PatchTodoItemStatus  - partial update (IsCompleted only)
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.PatchTodoItemStatus
    @Id          UNIQUEIDENTIFIER,
    @IsCompleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TodoItems
    SET
        IsCompleted  = @IsCompleted,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE Id = @Id;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT CAST(0 AS BIT) AS Success;
        RETURN;
    END

    SELECT
        Id,
        Title,
        Description,
        IsCompleted,
        CreatedAtUtc,
        UpdatedAtUtc
    FROM dbo.TodoItems
    WHERE Id = @Id;
END
GO

-- -----------------------------------------------------------------------------
-- 9. DeleteTodoItem
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.DeleteTodoItem
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.TodoItems
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO