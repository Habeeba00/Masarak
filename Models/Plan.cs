namespace Masarak.Models
{
    /// <summary>
    /// A subscription tier available to users (e.g. Free, Basic, Premium).
    ///
    /// Auth integration:
    ///   Only Admins create/modify plans (AdminOnly).
    ///   All authenticated users can read the plan catalogue (public or AnyAuthenticated).
    ///   Feature flags (HasAi, HasLiveClass, HasRecordings) are checked server-side
    ///   when a user attempts to access a gated feature — resolved through their
    ///   active Subscription → Plan.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Plan
    {
        public int      PlanId          { get; set; }
        public string   Name            { get; set; } = null!;
        public string?  Description     { get; set; }
        public decimal  PriceMonthly    { get; set; }
        public decimal? PriceYearly     { get; set; }
        public string   Currency        { get; set; } = "USD";
        public int      MaxSubjects     { get; set; } = -1;   // -1 = unlimited
        public bool     HasAi           { get; set; } = false;
        public bool     HasLiveClass    { get; set; } = true;
        public bool     HasRecordings   { get; set; } = true;
        public bool     IsActive        { get; set; } = true;

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
