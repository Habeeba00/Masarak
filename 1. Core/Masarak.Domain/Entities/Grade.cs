namespace Masarak.Domain.Entities
{
    /// <summary>
    /// Academic grade / year level (e.g. "Grade 7", "Grade 10").
    ///
    /// Auth integration:
    ///   No direct auth constraint. Accessible to Admin and Teacher.
    ///   Student grade is set at registration time (AuthService picks the lowest grade).
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Grade
    {
        public int    GradeId { get; set; }
        public string Name    { get; set; } = null!;
        public int    Level   { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Class>   Classes  { get; set; } = new List<Class>();
        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
