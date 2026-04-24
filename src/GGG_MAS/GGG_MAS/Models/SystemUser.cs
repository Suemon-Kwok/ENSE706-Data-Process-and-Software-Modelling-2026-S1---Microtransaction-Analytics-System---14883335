// Name : Suemon Kwok

// Student ID : 14883335

// SystemUser.cs — SystemUser class (UML)

// Internal staff user with role-based access control.

// FR13: Distinct roles. FR14: Restrict export/config.

// NFR03: Passwords stored as SHA-256 hashes only.

// What this file does
// represents a staff member logging into the system. Stores username, role, and a SHA-256 hash of their password (never the real password).
// Has Authenticate() to check login attempts, CanExport() and CanConfigure() to gate what they're allowed to do

// OOP Concepts
// Encapsulation and Principle of Least Privilege. Password is immediately hashed; _passwordHash is private.
// CanExport() and CanConfigure() gate access by role — each role gets only what it needs.

using System.Security.Cryptography;                                                                                                             // provides the SHA256 hashing algorithm        

using System.Text;                                                                                                                              // provides Encoding.UTF8 for converting strings to bytes

namespace GGG_MAS.Models                                                                                                                        // belongs to the shared model namespace
{
    /// <summary>
    
    /// Represents an internal GGG staff member using the MAS.
    
    /// Authentication and authorisation follow the principle of
    
    /// least privilege — roles grant only required capabilities.
    
    /// </summary>
    public class SystemUser
    {
        // Unique opaque user identifier
        public string UserId { get; private set; }                                                                                              // e.g. "U001"; used internally to identify the staff account

        // Login username (not an email for privacy minimisation)
        public string Username { get; private set; }                                                                                            // e.g. "analyst"; used on the login form

        // Stored as SHA-256 hex; plaintext is never persisted
        private string _passwordHash;                                                                                                           // private so nothing outside can read or compare the hash directly

        // Role determines what views and actions are available (FR13)
        
        public UserRole Role { get; private set; }                                                                                              // Analyst / MarketingManager / Developer / FinanceOfficer

        public SystemUser(string userId, string username,
                          string plainPassword, UserRole role)
        {
            UserId        = userId;                                                                                                             // stores the unique user ID

            Username      = username;                                                                                                           // stores the login username

            _passwordHash = HashPassword(plainPassword);                                                                                        // immediately hashes the plaintext password; never stored raw
            
            Role          = role;                                                                                                               // stores the assigned staff role
        }

        // Verifies a plaintext attempt against the stored hash (FR14)
        public bool Authenticate(string plainPassword) =>
            _passwordHash == HashPassword(plainPassword);                                                                                       // hashes the attempt and compares with the stored hash

        // Returns the user's assigned role
        public UserRole GetRole() => Role;                                                                                                      // used by the dashboard to check which views/actions to enable

        // FR14: Only Marketing Managers and Finance Officers can export (BR-09)
        public bool CanExport() =>
            
            Role == UserRole.MarketingManager ||                                                                                                // marketing managers need export for campaign data

            Role == UserRole.FinanceOfficer ||                                                                                                  // finance officers need export for financial report

            Role == UserRole.Analyst;                                                                                                           // analysts need export to share findings

        // FR14: Only Analysts and Developers can change system configuration
        public bool CanConfigure() =>
            
            Role == UserRole.Analyst ||                                                                                                         // analysts configure filters and report parameters

            Role == UserRole.Developer;                                                                                                         // developers can adjust system-level settings

        // SHA-256 one-way hash — never store plaintext passwords
        private static string HashPassword(string plain)
        {
            byte[] bytes  = SHA256.HashData(Encoding.UTF8.GetBytes(plain));                                                                     // converts the string to UTF-8 bytes then computes SHA-256

            return Convert.ToHexString(bytes).ToLower();                                                                                        // converts the raw hash bytes to a lowercase hex string
        }

        // Returns a readable summary for logging and display (never exposes the hash)
        public override string ToString() => $"{Username} [{Role}]";
    }
}
