namespace Masarak.Domain.Entities
{
    /// <summary>
    /// A question belonging to an Exam. Supports MCQ, True/False, Short Answer.
    ///
    /// Auth integration:
    ///   Teachers manage questions (TeacherOnly).
    ///   Students see questions during an active exam window only.
    ///   CorrectAns is NEVER returned in student-facing DTOs — enforce in the mapping layer.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Question
    {
        public int      QuestionId   { get; set; }
        public int      ExamId       { get; set; }   // FK → exams.ExamId
        public string   QuestionText { get; set; } = null!;
        public string   QuestionType { get; set; } = null!;   // MCQ|TrueFalse|ShortAnswer
        public string?  Options      { get; set; }             // JSON array for MCQ options
        public string?  CorrectAns   { get; set; }             // ⚠ strip from student DTOs
        public decimal  Marks        { get; set; } = 1;
        public int      OrderNum     { get; set; } = 1;

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Exam Exam { get; set; } = null!;
        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}
