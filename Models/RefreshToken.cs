namespace Masarak.Models
{
    /// <summary>
    /// Persisted refresh token record — Phase 2 addition.
    ///
    /// Design:
    ///   Each successful login or token-refresh creates one row.
    ///   The Token value is a cryptographically random 64-byte Base64 string.
    ///   JwtId ties this row to the exact Access Token it was issued with,
    ///   preventing cross-pair usage attacks.
    ///
    /// Rotation:
    ///   On each refresh: old row → IsUsed = true, ReplacedByToken = new token value.
    ///   Reuse of a used token → cascade revoke all tokens for this UserId.
    ///
    /// Relationship:
    ///   Many-to-one with User (a user can have multiple active sessions / devices).
    ///   User.RefreshTokens navigation property provides the full collection.
    /// </summary>
    public class RefreshToken
    {
        public int      Id               { get; set; }
        public int      UserId           { get; set; }   // FK → users.UserId
        public string   Token            { get; set; } = null!;
        public string   JwtId            { get; set; } = null!;   // matches JWT "jti" claim
        public bool     IsUsed           { get; set; } = false;
        public bool     IsRevoked        { get; set; } = false;
        public DateTime CreatedAt        { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt        { get; set; }
        public string?  ReplacedByToken  { get; set; }   // rotation audit trail
        public string?  RevokedReason    { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual User User { get; set; } = null!;
    }
}
