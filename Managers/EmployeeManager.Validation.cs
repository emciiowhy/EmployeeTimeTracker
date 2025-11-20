using System;
using System.Linq;
using EmployeeTimeTracker.Models;

namespace EmployeeTimeTracker.Managers
{
    public partial class EmployeeManager
    {
        public bool IdExists(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return employees.Any(e => e.EmployeeId.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public bool EmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return employees.Any(e => e.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
