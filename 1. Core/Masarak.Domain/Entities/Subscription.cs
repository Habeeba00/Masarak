namespace Masarak.Domain.Entities
{
    /// <summary>
    /// Records a User's subscription to a Plan over a billing period.
    ///
    /// Auth integration:
    ///   Users manage their own subscription (AnyAuthenticated, owner-scoped).
    ///   Admins manage all subscriptions (AdminOnly).
    ///   Feature-gate middleware checks:
    ///       db.Subscriptions
    ///         .Where(s => s.UserId == currentUserId && s.Status == "Active")
    ///         .Select(s => s.Plan)
    ///   to enforce HasAi / HasLiveClass gates.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Subscription
    {
        public int      SubscriptionId { get; set; }
        public int      UserId         { get; set; }   // FK → users.UserId
        public int      PlanId         { get; set; }   // FK → plans.PlanId
        public DateTime StartDate      { get; set; }
        public DateTime EndDate        { get; set; }
        public string   BillingCycle   { get; set; } = "Monthly";   // Monthly|Yearly
        public string   Status         { get; set; } = "Active";    // Active|Expired|Cancelled|Paused
        public bool     AutoRenew      { get; set; } = true;
        public DateTime CreatedAt      { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual User User { get; set; } = null!;
        public virtual Plan Plan { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
