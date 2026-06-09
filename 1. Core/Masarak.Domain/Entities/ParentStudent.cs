namespace Masarak.Domain.Entities
{
    /// <summary>
    /// Many-to-many join between Parent and Student.
    /// A parent can have multiple children; a student can have multiple parents.
    ///
    /// Auth integration:
    ///   When a Parent JWT hits /api/parent/children, the service queries:
    ///       db.ParentStudents
    ///         .Where(ps => ps.Parent.UserId == currentUserId)
    ///         .Select(ps => ps.Student)
    ///   Composite PK: (ParentId, StudentId) — no surrogate key needed.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class ParentStudent
    {
        public int     ParentId     { get; set; }   // FK → parents.ParentId
        public int     StudentId    { get; set; }   // FK → students.StudentId
        public string? Relationship { get; set; }   // e.g. "Father", "Mother", "Guardian"

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Parent  Parent  { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
