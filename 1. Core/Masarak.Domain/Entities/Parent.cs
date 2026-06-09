namespace Masarak.Domain.Entities
{
    /// <summary>
    /// Parent profile — one per User whose Role.Name == "Parent".
    ///
    /// Auth integration:
    ///   JWT "userid" claim resolves to this record via UserId.
    ///   [Authorize(Policy="ParentOnly")] guards parent endpoints.
    ///   Parents access child data through ParentStudent join rows.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Parent
    {
        public int ParentId { get; set; }
        public int UserId   { get; set; }   // FK → users.UserId

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ParentStudent> ParentStudents { get; set; } = new List<ParentStudent>();
    }
}
