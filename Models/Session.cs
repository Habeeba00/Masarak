namespace Masarak.Models
{
    /// <summary>
    /// A live or recorded class session within a TeachingAssignment.
    ///
    /// Auth integration:
    ///   Teachers create sessions (POST guarded by TeacherOnly policy).
    ///   Students in the linked Class can view sessions and submit attendance.
    ///   Parents can view session schedules for their children's classes.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Session
    {
        public int      SessionId    { get; set; }
        public int      AssignmentId { get; set; }   // FK → teaching_assignments.AssignmentId
        public string   Title        { get; set; } = null!;
        public DateTime StartTime    { get; set; }
        public DateTime EndTime      { get; set; }
        public string?  MeetingLink  { get; set; }
        public string?  RecordingUrl { get; set; }
        public string   Status       { get; set; } = "Scheduled";   // Scheduled|Ongoing|Completed|Cancelled

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual TeachingAssignment TeachingAssignment { get; set; } = null!;
        public virtual ICollection<Attendance> Attendances   { get; set; } = new List<Attendance>();
    }
}
