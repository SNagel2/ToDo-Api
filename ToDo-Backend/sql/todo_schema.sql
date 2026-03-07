-- =============================================================================
-- ToDo Application – SQL Schema & Stored Procedures
-- Target: SQL Server (T-SQL)
-- NOTE: The application currently uses an in-memory repository.
--       These scripts are provided for future database-backed persistence.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. Create database (run once; comment out if targeting an existing database)
-- -----------------------------------------------------------------------------
-- CREATE DATABASE ToDoDb;
-- GO
-- USE ToDoDb;
-- GO

-- -----------------------------------------------------------------------------
-- 2. ToDoItems table
-- -----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'ToDoItems'
)
BEGIN
    CREATE TABLE dbo.ToDoItems
    (
        Id            UNIQUEIDENTIFIER  NOT NULL  DEFAULT NEWSEQUENTIALID()  CONSTRAINT PK_ToDoItems PRIMARY KEY,
        Title         NVARCHAR(200)     NOT NULL,
        Description   NVARCHAR(2000)    NULL,
        IsCompleted   BIT               NOT NULL  DEFAULT 0,
        CreatedAtUtc  DATETIME2(7)      NOT NULL  DEFAULT SYSUTCDATETIME(),
        UpdatedAtUtc  DATETIME2(7)      NULL
    );
END
GO

-- Index to support filtering by status and ordering by creation date
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = 'IX_ToDoItems_IsCompleted_CreatedAtUtc'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ToDoItems_IsCompleted_CreatedAtUtc
        ON dbo.ToDoItems (IsCompleted, CreatedAtUtc DESC);
END
GO

-- -----------------------------------------------------------------------------
-- 3. Stored procedure: Get all to-do items (with filter + sort)
--    @Status     : 0 = All, 1 = Active (incomplete), 2 = Completed
--    @SortOrder  : 0 = Newest first, 1 = Oldest first
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_GetToDoItems
    @Status    TINYINT = 0,
    @SortOrder TINYINT = 0
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
    FROM dbo.ToDoItems
    WHERE
        (@Status = 0)                              -- All
        OR (@Status = 1 AND IsCompleted = 0)       -- Active
        OR (@Status = 2 AND IsCompleted = 1)       -- Completed
    ORDER BY
        CASE WHEN @SortOrder = 1 THEN CreatedAtUtc END ASC,     -- OldestFirst
        CASE WHEN @SortOrder = 0 THEN CreatedAtUtc END DESC;    -- NewestFirst
END
GO

-- -----------------------------------------------------------------------------
-- 4. Stored procedure: Get a single to-do item by ID
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_GetToDoItemById
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
    FROM dbo.ToDoItems
    WHERE Id = @Id;
END
GO

-- -----------------------------------------------------------------------------
-- 5. Stored procedure: Insert a new to-do item
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_InsertToDoItem
    @Id          UNIQUEIDENTIFIER,
    @Title       NVARCHAR(200),
    @Description NVARCHAR(2000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ToDoItems (Id, Title, Description, IsCompleted, CreatedAtUtc)
    VALUES (@Id, @Title, @Description, 0, SYSUTCDATETIME());

    -- Return the inserted row
    SELECT
        Id,
        Title,
        Description,
        IsCompleted,
        CreatedAtUtc,
        UpdatedAtUtc
    FROM dbo.ToDoItems
    WHERE Id = @Id;
END
GO

-- -----------------------------------------------------------------------------
-- 6. Stored procedure: Update an existing to-do item (full replace)
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_UpdateToDoItem
    @Id          UNIQUEIDENTIFIER,
    @Title       NVARCHAR(200),
    @Description NVARCHAR(2000) = NULL,
    @IsCompleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ToDoItems
    SET
        Title        = @Title,
        Description  = @Description,
        IsCompleted  = @IsCompleted,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE Id = @Id;

    -- Return affected rows count so caller can determine 404 vs 200
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

-- -----------------------------------------------------------------------------
-- 7. Stored procedure: Patch only the completion status
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_PatchToDoItemStatus
    @Id          UNIQUEIDENTIFIER,
    @IsCompleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ToDoItems
    SET
        IsCompleted  = @IsCompleted,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

-- -----------------------------------------------------------------------------
-- 8. Stored procedure: Delete a to-do item
-- -----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.usp_DeleteToDoItem
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.ToDoItems
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS AffectedRows;
END
GO
}