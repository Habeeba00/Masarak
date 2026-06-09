-- =============================================================================
-- Masarak EduPlatform — Phase 2 Auth Integration
-- SQL Schema Changes (idempotent — safe to re-run)
-- =============================================================================
-- Run order: execute this AFTER all Phase 1 tables already exist.
-- This script is equivalent to the EF Core migration
-- 20260604000000_Phase2_AuthIntegration.cs
-- =============================================================================

PRINT '=== Masarak Phase 2 Auth Integration — Starting ===';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 1: Widen users.PasswordHash  (255 → 500)
-- PBKDF2-SHA512 format: base64(16 salt) + ":" + base64(32 hash) ≈ 110 chars
-- 500 gives room for future algorithm upgrades.
-- ─────────────────────────────────────────────────────────────────────────────
IF COL_LENGTH('users', 'PasswordHash') < 500
BEGIN
    ALTER TABLE users ALTER COLUMN PasswordHash NVARCHAR(500) NOT NULL;
    PRINT 'STEP 1: users.PasswordHash widened to NVARCHAR(500). ✓';
END
ELSE
    PRINT 'STEP 1: users.PasswordHash already wide enough. Skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 2: Add EmailConfirmed column to users
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'EmailConfirmed'
)
BEGIN
    ALTER TABLE users ADD EmailConfirmed BIT NOT NULL DEFAULT 0;
    PRINT 'STEP 2: users.EmailConfirmed added. ✓';
END
ELSE
    PRINT 'STEP 2: users.EmailConfirmed already exists. Skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 3: Add FailedLoginCount column to users
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'FailedLoginCount'
)
BEGIN
    ALTER TABLE users ADD FailedLoginCount INT NOT NULL DEFAULT 0;
    PRINT 'STEP 3: users.FailedLoginCount added. ✓';
END
ELSE
    PRINT 'STEP 3: users.FailedLoginCount already exists. Skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 4: Add LockoutEnd column to users
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'users' AND COLUMN_NAME = 'LockoutEnd'
)
BEGIN
    ALTER TABLE users ADD LockoutEnd DATETIME2 NULL;
    PRINT 'STEP 4: users.LockoutEnd added. ✓';
END
ELSE
    PRINT 'STEP 4: users.LockoutEnd already exists. Skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 5: Create refresh_tokens table
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'refresh_tokens'
)
BEGIN
    CREATE TABLE refresh_tokens (
        Id               INT           NOT NULL IDENTITY(1,1),
        UserId           INT           NOT NULL,
        Token            NVARCHAR(500) NOT NULL,
        JwtId            NVARCHAR(100) NOT NULL,
        IsUsed           BIT           NOT NULL CONSTRAINT DF_rt_IsUsed     DEFAULT 0,
        IsRevoked        BIT           NOT NULL CONSTRAINT DF_rt_IsRevoked  DEFAULT 0,
        CreatedAt        DATETIME2     NOT NULL CONSTRAINT DF_rt_CreatedAt  DEFAULT GETDATE(),
        ExpiresAt        DATETIME2     NOT NULL,
        ReplacedByToken  NVARCHAR(500)     NULL,
        RevokedReason    NVARCHAR(255)     NULL,

        CONSTRAINT PK_refresh_tokens
            PRIMARY KEY (Id),

        CONSTRAINT FK_refresh_tokens_users
            FOREIGN KEY (UserId)
            REFERENCES users (UserId)
            ON DELETE NO ACTION
            ON UPDATE NO ACTION
    );
    PRINT 'STEP 5: refresh_tokens table created. ✓';
END
ELSE
    PRINT 'STEP 5: refresh_tokens already exists. Skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 6: Unique index on refresh_tokens.Token
-- Every refresh request looks up by Token value — this is the hot-path index.
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_refresh_tokens_Token'
      AND object_id = OBJECT_ID('refresh_tokens')
)
BEGIN
    CREATE UNIQUE INDEX UX_refresh_tokens_Token
        ON refresh_tokens (Token);
    PRINT 'STEP 6: UX_refresh_tokens_Token created. ✓';
END
ELSE
    PRINT 'STEP 6: UX_refresh_tokens_Token already exists. Skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 7: Index on refresh_tokens.UserId
-- Used during cascade-revoke ("revoke all tokens for user X").
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_refresh_tokens_UserId'
      AND object_id = OBJECT_ID('refresh_tokens')
)
BEGIN
    CREATE INDEX IX_refresh_tokens_UserId
        ON refresh_tokens (UserId);
    PRINT 'STEP 7: IX_refresh_tokens_UserId created. ✓';
END
ELSE
    PRINT 'STEP 7: IX_refresh_tokens_UserId already exists. Skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 8: Index on refresh_tokens.ExpiresAt
-- Used by cleanup jobs that prune expired tokens.
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_refresh_tokens_ExpiresAt'
      AND object_id = OBJECT_ID('refresh_tokens')
)
BEGIN
    CREATE INDEX IX_refresh_tokens_ExpiresAt
        ON refresh_tokens (ExpiresAt);
    PRINT 'STEP 8: IX_refresh_tokens_ExpiresAt created. ✓';
END
ELSE
    PRINT 'STEP 8: IX_refresh_tokens_ExpiresAt already exists. Skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- STEP 9: Ensure UX_users_Email exists (may be missing if Phase 1 was manual)
-- ─────────────────────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_users_Email'
      AND object_id = OBJECT_ID('users')
)
BEGIN
    CREATE UNIQUE INDEX UX_users_Email ON users (Email);
    PRINT 'STEP 9: UX_users_Email created. ✓';
END
ELSE
    PRINT 'STEP 9: UX_users_Email already exists. Skipped.';
GO

-- ─────────────────────────────────────────────────────────────────────────────
-- VERIFICATION QUERY — run after migration to confirm all objects exist
-- ─────────────────────────────────────────────────────────────────────────────
SELECT
    t.name              AS TableName,
    c.name              AS ColumnName,
    tp.name             AS DataType,
    c.max_length        AS MaxLength,
    c.is_nullable       AS IsNullable
FROM sys.tables t
JOIN sys.columns c  ON c.object_id = t.object_id
JOIN sys.types  tp  ON tp.user_type_id = c.user_type_id
WHERE t.name IN ('users', 'refresh_tokens')
  AND c.name IN (
        'PasswordHash', 'EmailConfirmed', 'FailedLoginCount', 'LockoutEnd',
        'Id', 'Token', 'JwtId', 'IsUsed', 'IsRevoked',
        'CreatedAt', 'ExpiresAt', 'ReplacedByToken', 'RevokedReason', 'UserId'
      )
ORDER BY t.name, c.column_id;
GO

PRINT '=== Phase 2 Auth Integration Complete ===';
GO
