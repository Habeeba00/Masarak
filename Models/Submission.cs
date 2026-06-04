namespace Masarak.Models
{
    /// <summary>
    /// A student's submission against an Assignment.
    /// Unique per (AssignmentId, StudentId) — one submission per assignment per student.
    ///
    /// Auth integration:
    ///   Students POST their own submission only (service enforces Student.UserId == currentUserId).
    ///   Teachers/Admins grade the submission (score + feedback fields).
    ///   Parents view submission status for linked children (read-only, ParentOnly).
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Submission
    {
        public int      SubmissionId { get; set; }
        public int      AssignmentId { get; set; }   // FK → assignments.AssignmentId
        public int      StudentId    { get; set; }   // FK → students.StudentId
        public DateTime SubmittedAt  { get; set; }
        public string?  FileUrl      { get; set; }
        public string?  AnswerText   { get; set; }
        public decimal? Score        { get; set; }
        public string?  Feedback     { get; set; }
        public string   Status       { get; set; } = "Submitted";   // Submitted|Graded|Late

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Assignment Assignment { get; set; } = null!;
        public virtual Student    Student    { get; set; } = null!;
    }
}
