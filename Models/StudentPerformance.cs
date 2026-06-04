namespace Masarak.Models
{
    /// <summary>
    /// Aggregated academic performance for a Student in a Subject for an AcademicYear.
    /// Unique per (StudentId, SubjectId, AcademicYear).
    /// Updated by a background job or trigger after each graded submission/exam.
    ///
    /// Auth integration:
    ///   Students view their own performance (StudentOnly, filter by Student.UserId).
    ///   Teachers view performance for students in their classes (TeacherOnly).
    ///   Parents view performance for linked children (ParentOnly).
    ///   Admins have full access (AdminOnly).
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class StudentPerformance
    {
        public int      PerformanceId  { get; set; }
        public int      StudentId      { get; set; }   // FK → students.StudentId
        public int      SubjectId      { get; set; }   // FK → subjects.SubjectId
        public string   AcademicYear   { get; set; } = null!;
        public decimal  AvgAssignment  { get; set; } = 0;
        public decimal  AvgExam        { get; set; } = 0;
        public decimal  AttendanceRate { get; set; } = 0;
        public decimal? FinalGrade     { get; set; }
        public string?  GradeLetter    { get; set; }   // A+, A, B+, B …
        public string?  Remarks        { get; set; }
        public DateTime UpdatedAt      { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Student Student { get; set; } = null!;
        public virtual Subject Subject { get; set; } = null!;
    }
}
