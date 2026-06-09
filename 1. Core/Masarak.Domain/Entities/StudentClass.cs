namespace Masarak.Domain.Entities
{
    /// <summary>
    /// Enrolls a Student in a Class for a given academic year.
    /// Composite PK: (StudentId, ClassId, AcademicYear).
    ///
    /// Auth integration:
    ///   Student JWT → db.StudentClasses.Where(sc => sc.Student.UserId == currentUserId)
    ///   IsCurrent flag narrows to the active enrolment.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class StudentClass
    {
        public int    StudentId    { get; set; }   // FK → students.StudentId
        public int    ClassId      { get; set; }   // FK → classes.ClassId
        public string AcademicYear { get; set; } = null!;   // e.g. "2025-2026"
        public bool   IsCurrent    { get; set; } = true;

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Student Student { get; set; } = null!;
        public virtual Class   Class   { get; set; } = null!;
    }
}
