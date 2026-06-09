namespace Masarak.Domain.Entities
{
    /// <summary>
    /// A homework/coursework assignment posted under a TeachingAssignment.
    ///
    /// Auth integration:
    ///   Teachers create assignments (TeacherOnly / AdminOrTeacher).
    ///   Students submit via Submission entity (StudentOnly).
    ///   Parents view assignment lists for their children (ParentOnly, read-only).
    ///
    /// No structural changes from Phase 1.
    /// Note: FK column name is "AssignmentRef" to avoid collision with AssignmentId PK.
    /// </summary>
    public class Assignment
    {
        public int      AssignmentId  { get; set; }
        public int      AssignmentRef { get; set; }   // FK → teaching_assignments.AssignmentId
        public string   Title         { get; set; } = null!;
        public string?  Description   { get; set; }
        public DateTime DueDate       { get; set; }
        public decimal  MaxScore      { get; set; } = 100;
        public DateTime CreatedAt     { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual TeachingAssignment      TeachingAssignment { get; set; } = null!;
        public virtual ICollection<Submission> Submissions        { get; set; } = new List<Submission>();
    }
}
