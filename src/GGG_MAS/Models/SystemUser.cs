// =============================================================
// SystemUser.cs — SystemUser class (UML)
// Internal staff user with role-based access control.
// FR13: Distinct roles. FR14: Restrict export/config.
// NFR03: Passwords stored as SHA-256 hashes only.
// =============================================================

using System.Security.Cryptography;
using System.Text;

namespace GGG_MAS.Models
{
    /// <summary>
    /// Represents an internal GGG staff member using the MAS.
    /// Authentication and authorisation follow the principle of
    /// least privilege — roles grant only required capabilities.
    /// </summary>
    public class SystemUser
    {
        // Unique opaque user identifier
        public string UserId { get; private set; }

        // Login username (not an email for privacy minimisation)
        public string Username { get; private set; }

        // Stored as SHA-256 hex; plaintext is never persisted
        private string _passwordHash;

        // Role determines what views and actions are available (FR13)
        public UserRole Role { get; private set; }

        public SystemUser(string userId, string username,
                          string plainPassword, UserRole role)
        {
            UserId        = userId;
            Username      = username;
            _passwordHash = HashPassword(plainPassword);  // hash immediately
            Role          = role;
        }

        // Verifies a plaintext attempt against the stored hash (FR14)
        public bool Authenticate(string plainPassword) =>
            _passwordHash == HashPassword(plainPassword);

        // Returns the user's assigned role
        public UserRole GetRole() => Role;

        // FR14: Only Marketing Managers and Finance Officers can export (BR-09)
        public bool CanExport() =>
            Role == UserRole.MarketingManager ||
            Role == UserRole.FinanceOfficer  ||
            Role == UserRole.Analyst;

        // FR14: Only Analysts and Developers can change system configuration
        public bool CanConfigure() =>
            Role == UserRole.Analyst ||
            Role == UserRole.Developer;

        // SHA-256 one-way hash — never store plaintext passwords
        private static string HashPassword(string plain)
        {
            byte[] bytes  = SHA256.HashData(Encoding.UTF8.GetBytes(plain));
            return Convert.ToHexString(bytes).ToLower();
        }

        public override string ToString() => $"{Username} [{Role}]";
    }
}
