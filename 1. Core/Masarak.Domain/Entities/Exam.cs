namespace Masarak.Domain.Entities
{
    /// <summary>
    /// An exam event under a TeachingAssignment (Quiz, MidTerm, Final, etc.).
    ///
    /// Auth integration:
    ///   Teachers create exams (TeacherOnly).
    ///   Students take exams via StudentExam (StudentOnly, bounded by exam window).
    ///   Parents view exam schedule and child results (ParentOnly, read-only).
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Exam
    {
        public int      ExamId       { get; set; }
        public int      AssignmentId { get; set; }   // FK → teaching_assignments.AssignmentId
        public string   Title        { get; set; } = null!;
        public DateTime StartTime    { get; set; }
        public DateTime EndTime      { get; set; }
        public int      DurationMins { get; set; }
        public decimal  MaxScore     { get; set; } = 100;
        public string   ExamType     { get; set; } = "Quiz";   // Quiz|MidTerm|Final|Practice

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual TeachingAssignment         TeachingAssignment { get; set; } = null!;
        public virtual ICollection<Question>      Questions          { get; set; } = new List<Question>();
        public virtual ICollection<StudentExam>   StudentExams       { get; set; } = new List<StudentExam>();
    }
}
