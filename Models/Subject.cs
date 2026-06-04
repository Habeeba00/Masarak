namespace Masarak.Models
{
    /// <summary>
    /// An academic subject belonging to a Grade (e.g. "Mathematics", "Physics").
    ///
    /// Auth integration:
    ///   Admin manages subjects.
    ///   Teachers access subjects through their TeachingAssignment rows.
    ///   Students access subjects through their StudentClass → TeachingAssignment chain.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Subject
    {
        public int     SubjectId   { get; set; }
        public int     GradeId     { get; set; }   // FK → grades.GradeId
        public string  Name        { get; set; } = null!;
        public string  Code        { get; set; } = null!;   // unique, e.g. "MATH-07"
        public string? Description { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Grade  Grade  { get; set; } = null!;
        public virtual ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();
        public virtual ICollection<StudentPerformance> StudentPerformances  { get; set; } = new List<StudentPerformance>();
    }
}
