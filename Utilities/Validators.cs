using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EmployeeTimeTracker.Utilities
{
    public static class Validators
    {
        // -----------------------------------------
        // EMAIL VALIDATION
        // -----------------------------------------

        private static readonly Regex EmailPattern = new(
            @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$",
            RegexOptions.Compiled);

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim();

            return EmailPattern.IsMatch(email);
        }

        // -----------------------------------------
        // EMPLOYEE ID VALIDATION
        // Allowed: letters, digits, hyphens, underscores
        // -----------------------------------------

        private static readonly Regex EmployeeIdPattern = new(
            @"^[A-Za-z0-9\-_]+$",
            RegexOptions.Compiled);

        /// <summary>
        /// New helper: ensures ID uses only valid characters.
        /// Does NOT replace your existing logic — only adds safety.
        /// </summary>
        public static bool IsValidEmployeeIdFormat(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            // prevent absurdly huge IDs
            if (id.Length > 40)
                return false;

            return EmployeeIdPattern.IsMatch(id);
        }

        // -----------------------------------------
        // NAME VALIDATION
        // Allowed: letters + spaces only
        // -----------------------------------------

        private static readonly Regex NamePattern = new(
            @"^[A-Za-z ]+$",
            RegexOptions.Compiled);

        /// <summary>
        /// NEW METHOD: Validates names to avoid numeric or symbol names.
        /// Safe to add — no existing functionality depending on name validation.
        /// </summary>
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();

            // prevent all-space names
            if (name.All(char.IsWhiteSpace))
                return false;

            return NamePattern.IsMatch(name);
        }
    }
}
