namespace Masarak.Models
{
    /// <summary>
    /// A student's answer to a single Question within a StudentExam attempt.
    /// Unique per (StudentExamId, QuestionId).
    ///
    /// Auth integration:
    ///   Students submit answers during an active exam (StudentOnly).
    ///   MarksAwarded is computed/set by grading logic or auto-grader (TeacherOnly/AdminOnly).
    ///   IsCorrect is hidden from student DTOs until exam is officially graded.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class StudentAnswer
    {
        public int      AnswerId       { get; set; }
        public int      StudentExamId  { get; set; }   // FK → student_exams.StudentExamId
        public int      QuestionId     { get; set; }   // FK → questions.QuestionId
        public string?  AnswerText     { get; set; }
        public bool?    IsCorrect      { get; set; }
        public decimal? MarksAwarded   { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual StudentExam StudentExam { get; set; } = null!;
        public virtual Question    Question    { get; set; } = null!;
    }
}
