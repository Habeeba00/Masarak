namespace Masarak.Domain.Entities
{
    /// <summary>
    /// Assigns a Teacher to teach a Subject to a Class in a given AcademicYear.
    /// Unique constraint: (TeacherId, SubjectId, ClassId, AcademicYear).
    ///
    /// Auth integration:
    ///   Teacher JWT → db.TeachingAssignments.Where(ta => ta.Teacher.UserId == currentUserId)
    ///   This is the primary authorization scope for all teacher-level data:
    ///   Sessions, Assignments, Exams all hang off this entity.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class TeachingAssignment
    {
        public int      AssignmentId { get; set; }
        public int      TeacherId    { get; set; }   // FK → teachers.TeacherId
        public int      SubjectId    { get; set; }   // FK → subjects.SubjectId
        public int      ClassId      { get; set; }   // FK → classes.ClassId
        public string   AcademicYear { get; set; } = null!;
        public DateTime CreatedAt    { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Teacher Teacher { get; set; } = null!;
        public virtual Subject Subject { get; set; } = null!;
        public virtual Class   Class   { get; set; } = null!;
        public virtual ICollection<Session>    Sessions    { get; set; } = new List<Session>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public virtual ICollection<Exam>       Exams       { get; set; } = new List<Exam>();
    }
}
