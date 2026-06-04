namespace Masarak.Models
{
    /// <summary>
    /// Records a Student's presence in a Session.
    /// Unique per (SessionId, StudentId).
    ///
    /// Auth integration:
    ///   Teachers mark attendance (AdminOrTeacher policy).
    ///   Students see their own attendance record only
    ///       (service filters by Student.UserId == currentUserId).
    ///   Parents see attendance for their linked children.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Attendance
    {
        public int       AttendanceId { get; set; }
        public int       SessionId    { get; set; }   // FK → sessions.SessionId
        public int       StudentId    { get; set; }   // FK → students.StudentId
        public string    Status       { get; set; } = "Present";   // Present|Absent|Late|Excused
        public DateTime? JoinedAt     { get; set; }
        public DateTime? LeftAt       { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Session Session { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
