namespace Masarak.Models
{
    /// <summary>
    /// Lookup table for system roles: Admin | Teacher | Student | Parent
    /// Auth integration: RoleId FK lives on User; Role.Name is written into
    /// the JWT "role" and ClaimTypes.Role claims at login time.
    /// No structural changes from Phase 1.
    /// </summary>
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = null!;

        // ── Navigation ──────────────────────────────────────────────────────
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
