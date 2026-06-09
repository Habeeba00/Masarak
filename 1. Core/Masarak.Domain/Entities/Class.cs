namespace Masarak.Domain.Entities
{
    /// <summary>
    /// A classroom grouping within a Grade (e.g. "7A", "10B").
    ///
    /// Auth integration:
    ///   Admin creates/manages classes.
    ///   Teachers see only their assigned classes (TeachingAssignment rows).
    ///   Students see their enrolled class (StudentClass rows).
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Class
    {
        public int    ClassId  { get; set; }
        public int    GradeId  { get; set; }   // FK → grades.GradeId
        public string Name     { get; set; } = null!;
        public int    Capacity { get; set; } = 30;

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Grade  Grade  { get; set; } = null!;
        public virtual ICollection<StudentClass>       StudentClasses       { get; set; } = new List<StudentClass>();
        public virtual ICollection<TeachingAssignment> TeachingAssignments  { get; set; } = new List<TeachingAssignment>();
    }
}
