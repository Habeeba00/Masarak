namespace Masarak.Models
{
    /// <summary>
    /// A payment transaction linked to a Subscription.
    ///
    /// Auth integration:
    ///   Users view their own payment history (owner-scoped, AnyAuthenticated).
    ///   Only Admins can view all payments and issue refunds (AdminOnly).
    ///   GatewayTxnId is used to correlate with external payment gateway records.
    ///
    /// No structural changes from Phase 1.
    /// </summary>
    public class Payment
    {
        public int       PaymentId      { get; set; }
        public int       SubscriptionId { get; set; }   // FK → subscriptions.SubscriptionId
        public decimal   Amount         { get; set; }
        public string    Currency       { get; set; } = "USD";
        public string    Gateway        { get; set; } = null!;    // e.g. "Stripe", "PayPal", "Fawry"
        public string?   GatewayTxnId   { get; set; }
        public string    Status         { get; set; } = null!;    // Pending|Completed|Failed|Refunded
        public DateTime? PaidAt         { get; set; }
        public DateTime  CreatedAt      { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual Subscription Subscription { get; set; } = null!;
    }
}
