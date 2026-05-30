// Name: Suemon Kwok
// Student ID: 14883335
// AuthService.cs — Singleton pattern (Task 6 / GoF Pattern 3)
//
// Design pattern: Singleton (GoF Creational)
//
// WHY THIS PATTERN HERE:
// Phase 1 AuthService required the user list to be passed in via constructor,
// meaning a second AuthService could be created with a different user list —
// a silent split-brain bug. The Singleton guarantees exactly one AuthService
// exists for the lifetime of the application process.
//
// Thread safety: double-checked locking with volatile field is the standard
// thread-safe Singleton for .NET, ready for a future ASP.NET migration (NFR04).

/*
What design pattern is used and why?
Design pattern: Singleton — GoF Creational category.
The Singleton pattern is used in AuthService.cs because AuthService acts as the application-wide session store. It holds the current authenticated user (CurrentUser), 
verifies passwords, and controls who can export or configure the system. In Phase 1, AuthService was constructed with new AuthService(systemUsers), which meant nothing prevented a second AuthService from being created 
giving it its own separate CurrentUser. This is the classic split-brain bug the Singleton solves.

 */

/*
What does the Singleton pattern do?
The Singleton pattern ensures that a class has exactly one instance for the entire lifetime of the application, and provides a single global access point to that instance via GetInstance().
In this project: every call to AuthService.GetInstance() returns the same object. LoginForm and MainDashboardForm both call GetInstance() and always get the same session store. 
This is proved in PatternUsageDemo.cs with ReferenceEquals(auth1, auth2) == true.

*/

/*
Why Singleton and not another creational pattern?
Factory Method was considered but rejected. Factory Method creates new objects; the Singleton deliberately prevents new objects. The problem is not how to create AuthService — it is how to guarantee only one exists. Static class was also considered but rejected because static classes cannot implement interfaces or be mocked for testing.

*/

/*
Singleton: Eager vs Lazy loading
Lazy loading — the instance is created only when GetInstance() is called for the first time (on demand, at runtime). 
The field _instance starts as null and is only assigned inside the lock when null is detected.

Eager loading — would initialise the instance at class-load time: private static AuthService _instance = new AuthService();. 
This runs before Main() and wastes resources if AuthService is never needed.

This project uses lazy loading deliberately: AuthService is only created when the login form first calls GetInstance(). 
This saves startup time and follows the principle of deferring work until it is actually needed.

*/

/*
Thread safety in Singleton — double-checked locking
Thread safety means multiple threads can call the same code simultaneously without corrupting shared state.
Without a lock, two threads could both reach if (_instance is null) at the same time, both find it null, and both create a new AuthService — breaking the Singleton guarantee.
This implementation uses double-checked locking:
•	1. First check (outside lock): if _instance is already set, return it immediately — fast path, no lock overhead.
•	2. Lock: only one thread can enter this block at a time.
•	3. Second check (inside lock): re-check because another thread might have created it between check 1 and acquiring the lock.
The volatile keyword ensures the write to _instance is immediately visible to all threads — without it, a thread could read a cached (stale) null value even after another thread has written the instance.

Why volatile? Answer: without volatile, the CPU or compiler can reorder memory operations, 
so one thread might see _instance as null even after another thread has written it. volatile prevents this CPU-level caching issue
*/

/*
Private constructor — what it is and why it matters
A constructor is a special method that runs when you create an object with new ClassName(). It initialises the object's fields.
A private constructor does the same thing but cannot be called from outside the class. In AuthService: private AuthService() means new AuthService() from Program.cs or any other class is a compile error.
The difference: a public constructor allows anyone to create as many instances as they want. 
A private constructor forces all object creation through the controlled GetInstance() method, which enforces the one-instance rule.

What stops someone calling new AuthService()?' Answer: the private keyword on the constructor. 
The compiler enforces this — it is not a runtime check.
*/

/*
Singleton is commonly used for database connections — why?
Database connections are expensive to create. Each connection involves a network handshake, authentication, and resource allocation. Creating a new connection for every request wastes time and exhausts the database's connection pool.

AuthService in this project is analogous: it holds the single session store for the entire application. 
If two AuthService instances existed, one login on form A would not be visible to form B — exactly the same problem as two database connections with out-of-sync state.

Real-world examples of Singleton for database work: Entity Framework DbContext (one per request), 
logging services (one log file writer), configuration managers (one settings reader). All share the same need: one shared resource, one controlled access point.

*/

/*
Tight coupling — what it is, why it is bad, how Singleton helps
Tight coupling means one class directly depends on the concrete implementation of another. Phase 1 Program.cs had: var auth = new AuthService(systemUsers). 
Program.cs was tightly coupled to AuthService's constructor signature. Changing the constructor broke Program.cs.

Why tight coupling is bad: it creates a chain reaction. One change forces changes elsewhere. 
It makes code harder to test (you cannot swap a fake AuthService), harder to refactor, and harder to extend.

How Singleton reduces coupling: AuthService.GetInstance() has no parameters. Program.cs no longer depends on the constructor signature. 
Any class needing authentication calls GetInstance() — they all get the same object without knowing how it was constructed.

*/

/*
Advantages of Singleton
Guarantees exactly one instance — no accidental split-brain bugs
Saves resources — instance created once, reused everywhere
Centralised access point — no constructor arguments needed
Thread-safe with double-checked locking — safe for future ASP.NET migration

*/

/*
Disadvantages of Singleton
Hard to unit test: static instance persists across tests unless Logout() is called
Introduces global state — any code anywhere can access CurrentUser
Not suitable for concurrent multi-user web apps without refactoring to scoped DI
Can be misused as a 'god object' that accumulates unrelated responsibilities

*/

using System.Security.Cryptography;
using System.Text;
using GGG_MAS.Models;

namespace GGG_MAS.Services
{
    /// <summary>
    /// Singleton application-wide authentication and session manager.
    /// Obtain the instance via <see cref="GetInstance"/>; do not use new().
    /// </summary>
    public sealed class AuthService
    {
        // Singleton infrastructure

        // volatile: guarantees the write to _instance is immediately visible to ALL threads.
        // Without volatile, a CPU core could cache a stale null value and bypass the lock.
        // static: belongs to the class itself, not to any instance — shared across the whole application.
        // AuthService?: the ? means nullable — starts as null until GetInstance() creates it (lazy loading).

        private static volatile AuthService? _instance;


        // _lock is the object used as the mutex — only one thread can hold this lock at a time.
        // readonly: the lock object itself can never be replaced after initialisation.

        private static readonly object _lock = new();

        public static AuthService GetInstance()
        {
            // FIRST CHECK (no lock — fast path):
            // If _instance already exists, skip the lock entirely.
            // This makes 99% of calls very fast — no synchronisation overhead.
            if (_instance is null)
            {
                // LOCK: only one thread enters this block at a time.
                // Other threads wait here until the lock is released.
                lock (_lock)
                {
                    // SECOND CHECK (inside lock — safety net):
                    // Thread A and Thread B may both pass the first null check simultaneously.
                    // Thread A acquires the lock first and creates the instance.
                    // Thread B then enters the lock — without this second check,
                    // it would create a second instance, breaking the Singleton.
                    if (_instance is null)
                        _instance = new AuthService();  // private constructor called here — only place in the codebase                                            
                }
            }
            return _instance;   // always the same object reference — guaranteed
        }

        // Private constructor — enforces Singleton; seeds users using the
        // same data SeedData.CreateSystemUsers() would produce.
        // PRIVATE CONSTRUCTOR:
        // private = cannot be called with new AuthService() from anywhere outside this class.
        // The compiler enforces this — it is a compile error, not a runtime error.
        // This is what physically prevents a second AuthService from being created.
        // Seeds four demo users that match the original SeedData.CreateSystemUsers() output.
        private AuthService()
        {
            // Initialise the user list directly — no dependency on SeedData.
            // Each SystemUser stores the password as a SHA-256 hash internally,
            // never as plaintext (encapsulation of sensitive data).
            _users = new List<SystemUser>
            {
                // SystemUser(userId, username, plainPassword, role)
                // The plain password is hashed inside SystemUser's constructor — not stored here.
                new SystemUser("U001", "analyst",   "analyst123",  UserRole.Analyst),                           // full system access
                new SystemUser("U002", "marketing", "mktg123",     UserRole.MarketingManager),                  // reports + export
                new SystemUser("U003", "developer", "dev123",      UserRole.Developer),                         // item data + underperformance
                new SystemUser("U004", "finance",   "finance123",  UserRole.FinanceOfficer),                    // revenue reports + export
            };
        }

        // Session state
        // CurrentUser: holds the authenticated user for the duration of the session.
        // public get: any class can READ who is logged in.
        // private set: only AuthService itself can WRITE this — no external code can hijack the session.
        // SystemUser?: nullable — null means no user is currently logged in.
        public SystemUser? CurrentUser { get; private set; }

        // Matches original property name used throughout the codebase
        // IsLoggedIn: convenience property — true when CurrentUser is not null.
        // Uses the null-conditional pattern: CurrentUser != null evaluates to bool.
        // Used by MainDashboardForm to guard role-specific views.
        public bool IsLoggedIn => CurrentUser != null;

        // User store
        // _users: the in-memory list of all known system users.
        // private: only Login() can search this list — no external code can enumerate users.
        // readonly: the list reference cannot be replaced after the constructor runs,
        //           though items can still be added/removed (this is a design limitation).
        private readonly List<SystemUser> _users;

        // Authentication
        /// <summary>
        /// Authenticates a user by username and password.
        /// Returns true and sets CurrentUser on success.
        /// Returns false without revealing whether the username or password was wrong (security best practice).
        /// </summary>
        public bool Login(string username, string password)
        {
            // Guard clause: reject empty credentials immediately.
            // IsNullOrWhiteSpace catches null, "", and "   " (spaces only).
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
                return false;

            // Search the user list for a matching username.
            // OrdinalIgnoreCase: "Analyst" and "analyst" both match — case-insensitive.
            // .Trim(): strips leading/trailing spaces the user may have typed accidentally.
            // FirstOrDefault: returns null if no match found — never throws an exception.
            var user = _users.FirstOrDefault(
                u => u.Username.Equals(username.Trim(),
                                       StringComparison.OrdinalIgnoreCase));

            // Delegates to SystemUser.Authenticate() which handles SHA-256 internally
            // Authenticate() is on SystemUser — it computes SHA-256(password) internally
            // and compares to the stored hash. Plain password never leaves this method.
            // Short-circuit evaluation: if user is null, Authenticate() is never called.
            if (user != null && user.Authenticate(password))
            {
                CurrentUser = user;         // session established — user is now logged in
                return true;
            }

            // Generic false — deliberately does not say "wrong username" or "wrong password".
            // Revealing which field failed helps attackers enumerate valid usernames.
            return false;
        }

        // Logout: clears the session by setting CurrentUser back to null.
        // => is an expression body — shorthand for { CurrentUser = null; }
        // Called by MainDashboardForm when the user clicks the Logout button.
        public void Logout() => CurrentUser = null;

        // Permission helpers (delegate to SystemUser)
        // CanExport: returns true for Analyst, MarketingManager, and FinanceOfficer.
        // Delegates to SystemUser.CanExport() — the role logic lives on the user object.
        // ?? false: if CurrentUser is null (not logged in), CanExport() returns false safely.
        // Used by MainDashboardForm to show or hide the Export Report button.
        public bool CanExport()     => CurrentUser?.CanExport()     ?? false;

        // CanConfigure: returns true for Analyst and Developer only.
        // Used to show or hide the Configure Threshold panel on the dashboard.
        public bool CanConfigure()  => CurrentUser?.CanConfigure()  ?? false;
    }
}
