namespace Masarak.Domain.Entities
{
    /// <summary>
    /// Records a Student's attempt at an Exam.
    /// Unique per (ExamId, StudentId) — one attempt per exam per student.
    ///
    /// Auth integration:
    ///   Created when a Student starts an exam (StudentOnly).
    ///   TotalScore is computed server-side on submission (not trusted from client).
    ///   Teachers/Admins can review all StudentExam rows for their exam.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class StudentExam
    {
        public int       StudentExamId { get; set; }
        public int       ExamId        { get; set; }   // FK → exams.ExamId
        public int       StudentId     { get; set; }   // FK → students.StudentId
        public DateTime? StartedAt     { get; set; }
        public DateTime? SubmittedAt   { get; set; }
        public decimal?  TotalScore    { get; set; }
        public string    Status        { get; set; } = "Pending";   // Pending|InProgress|Submitted|Graded

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Exam    Exam    { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}
