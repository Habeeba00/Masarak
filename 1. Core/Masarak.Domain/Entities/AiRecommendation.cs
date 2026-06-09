namespace Masarak.Domain.Entities
{
    /// <summary>
    /// AI-generated study recommendation targeted at a specific Student.
    ///
    /// Auth integration:
    ///   Students read their own recommendations (StudentOnly, filter by Student.UserId).
    ///   Parents read recommendations for linked children (ParentOnly).
    ///   Generation is triggered by Admin/system jobs (AdminOnly write).
    ///   IsRead / IsDismissed are updated by the student themselves.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class AiRecommendation
    {
        public int      RecommendationId { get; set; }
        public int      StudentId        { get; set; }   // FK → students.StudentId
        public DateTime GeneratedAt      { get; set; }
        public string   RecType          { get; set; } = null!;   // e.g. "StudyTip", "Resource", "Alert"
        public string?  ReferenceType    { get; set; }             // e.g. "Subject", "Assignment"
        public int?     ReferenceId      { get; set; }
        public string   Reason           { get; set; } = null!;
        public string?  ActionUrl        { get; set; }
        public bool     IsRead           { get; set; } = false;
        public bool     IsDismissed      { get; set; } = false;

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Student Student { get; set; } = null!;
    }
}
