namespace Masarak.Domain.Entities
{
    /// <summary>
    /// In-app notification delivered to any User regardless of role.
    ///
    /// Auth integration:
    ///   Every authenticated user reads their own notifications:
    ///       db.Notifications.Where(n => n.UserId == currentUserId)
    ///   The "userid" JWT claim provides the filter key.
    ///   Only the owner can mark IsRead / set ReadAt (enforced in service layer).
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Notification
    {
        public int       NotificationId { get; set; }
        public int       UserId         { get; set; }   // FK → users.UserId
        public string    Title          { get; set; } = null!;
        public string    Body           { get; set; } = null!;
        public string    NotifType      { get; set; } = null!;   // e.g. "Assignment", "Exam", "Grade"
        public string?   ReferenceType  { get; set; }
        public int?      ReferenceId    { get; set; }
        public bool      IsRead         { get; set; } = false;
        public DateTime  SentAt         { get; set; }
        public DateTime? ReadAt         { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual User User { get; set; } = null!;
    }
}
