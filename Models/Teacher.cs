namespace Masarak.Models
{
    /// <summary>
    /// Teacher profile — one per User whose Role.Name == "Teacher".
    ///
    /// Auth integration:
    ///   JWT "userid" claim resolves to this record via UserId.
    ///   [Authorize(Policy="TeacherOnly")] and [Authorize(Policy="AdminOrTeacher")]
    ///   guard teacher-scoped endpoints.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Teacher
    {
        public int      TeacherId      { get; set; }
        public int      UserId         { get; set; }   // FK → users.UserId
        public string?  Specialization { get; set; }
        public DateTime HiringDate     { get; set; }
        public string?  Bio            { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual User User { get; set; } = null!;
        public virtual ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();
    }
}
