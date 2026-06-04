using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masarak.Migrations.EfMigrations
{
    /// <inheritdoc />
    public partial class Phase2_AuthIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Widen PasswordHash column ────────────────────────────────────
            // PBKDF2 format is "base64(16-byte-salt):base64(32-byte-hash)" ≈ 110 chars,
            // but we use 500 for headroom against future algorithm changes.
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            // ── 2. Add Phase 2 auth columns to users ─────────────────────────────
            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginCount",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "users",
                type: "datetime2",
                nullable: true);

            // ── 3. Create refresh_tokens table ────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId          = table.Column<int>(type: "int",           nullable: false),
                    Token           = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    JwtId           = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsUsed          = table.Column<bool>(type: "bit",          nullable: false, defaultValue: false),
                    IsRevoked       = table.Column<bool>(type: "bit",          nullable: false, defaultValue: false),
                    CreatedAt       = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ExpiresAt       = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RevokedReason   = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── 4. Indexes on refresh_tokens ──────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "UX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ExpiresAt",
                table: "refresh_tokens",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop refresh_tokens first (FK dependency on users)
            migrationBuilder.DropTable(name: "refresh_tokens");

            // Remove Phase 2 columns from users
            migrationBuilder.DropColumn(name: "LockoutEnd",       table: "users");
            migrationBuilder.DropColumn(name: "FailedLoginCount", table: "users");
            migrationBuilder.DropColumn(name: "EmailConfirmed",   table: "users");

            // Restore narrow PasswordHash
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
