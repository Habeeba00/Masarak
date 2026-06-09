-- =============================================================================
-- Phase 2 Migration: refresh_tokens table
-- Run this AFTER your Phase 1 tables already exist.
-- If you are starting fresh, run your Phase 1 migration first,
-- then run this script.
-- =============================================================================

-- 1. Widen PasswordHash column to hold PBKDF2 hashes (was 255, now 500)
IF COL_LENGTH('users', 'PasswordHash') < 500
BEGIN
    ALTER TABLE users ALTER COLUMN PasswordHash NVARCHAR(500) NOT NULL;
    PRINT 'users.PasswordHash widened to 500.';
END

-- 2. Create refresh_tokens table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'refresh_tokens')
BEGIN
    CREATE TABLE refresh_tokens (
        Id               INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
        UserId           INT             NOT NULL,
        Token            NVARCHAR(500)   NOT NULL,
        JwtId            NVARCHAR(100)   NOT NULL,
        IsUsed           BIT             NOT NULL DEFAULT 0,
        IsRevoked        BIT             NOT NULL DEFAULT 0,
        CreatedAt        DATETIME2       NOT NULL DEFAULT GETDATE(),
        ExpiresAt        DATETIME2       NOT NULL,
        ReplacedByToken  NVARCHAR(500)   NULL,
        RevokedReason    NVARCHAR(255)   NULL,

        CONSTRAINT FK_RefreshTokens_Users
            FOREIGN KEY (UserId) REFERENCES users(UserId)
            ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX UX_refresh_tokens_Token
        ON refresh_tokens (Token);

    CREATE INDEX IX_refresh_tokens_UserId
        ON refresh_tokens (UserId);

    CREATE INDEX IX_refresh_tokens_ExpiresAt
        ON refresh_tokens (ExpiresAt);

    PRINT 'refresh_tokens table created.';
END
ELSE
BEGIN
    PRINT 'refresh_tokens table already exists. Skipped.';
END

-- 3. Optional: index on users.Email (likely already exists from Phase 1)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_users_Email' AND object_id = OBJECT_ID('users')
)
BEGIN
    CREATE UNIQUE INDEX UX_users_Email ON users(Email);
    PRINT 'UX_users_Email index created.';
END

PRINT 'Phase 2 migration complete.';
