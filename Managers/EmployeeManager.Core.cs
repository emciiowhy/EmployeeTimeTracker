using System;
using System.Collections.Generic;
using System.Linq;
using EmployeeTimeTracker.Models;
using EmployeeTimeTracker.Utilities;

namespace EmployeeTimeTracker.Managers
{
    /// <summary>
    /// Core behavior for Employee Manager: Add, Remove, Get, Search, Reports.
    /// Validation and File I/O handled in other partial classes.
    /// </summary>
    public partial class EmployeeManager
    {
        private readonly List<Employee> employees = new();

        public event Action<string>? OnEmployeeChanged;

        // ------------------------------
        // Validation Helpers (Core-level)
        // ------------------------------
        private static bool IsValidEmployeeId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            if (id.Length > 40) return false; // Prevent stupid long IDs

            // IDs should be alphanumeric only
            return id.All(c => char.IsLetterOrDigit(c));
        }

        private static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            // Prevent names like: "123213123", "asd!@#", "MC213"
            return name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }

        // ------------------------------
        // Core Logic
        // ------------------------------
        public void AddEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // Validate ID rules
            if (!IsValidEmployeeId(employee.EmployeeId))
                throw new ArgumentException("Employee ID must be alphanumeric and cannot contain spaces or symbols.");

            // Validate name rules
            if (!IsValidName(employee.Name))
                throw new ArgumentException("Name can only contain letters and spaces.");

            // Validate email
            if (!Validators.IsValidEmail(employee.Email))
                throw new ArgumentException("Invalid email format.");

            // Uniqueness checks
            if (IdExists(employee.EmployeeId))
                throw new InvalidOperationException($"Employee ID '{employee.EmployeeId}' already exists.");

            if (EmailExists(employee.Email))
                throw new InvalidOperationException($"Email '{employee.Email}' is already used by another employee.");

            // Add to list
            employees.Add(employee);

            OnEmployeeChanged?.Invoke(
                $"Employee {employee.Name} ({employee.EmployeeId}) added."
            );
        }

        public bool RemoveEmployee(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            var emp = FindEmployeeById(id);

            if (emp == null)
                return false;

            employees.Remove(emp);

            OnEmployeeChanged?.Invoke(
                $"Employee {emp.Name} ({emp.EmployeeId}) removed."
            );

            return true;
        }

        public Employee? FindEmployeeById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return employees
                .FirstOrDefault(e => e.EmployeeId.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public List<Employee> GetAllEmployees() => employees.ToList();

        public void DisplayAllEmployees()
        {
            if (employees.Count == 0)
            {
                Console.WriteLine("No employees found.");
                return;
            }

            Console.WriteLine("\n=== ALL EMPLOYEES ===");

            foreach (var emp in employees.OrderBy(e => e.Name))
            {
                emp.DisplayInfo();
                Console.WriteLine("---");
            }
        }

        public void GenerateSummaryReport()
        {
            Console.WriteLine("\n=== EMPLOYEE SUMMARY REPORT ===");

            int total = employees.Count;
            int fullTime = employees.Count(e => e is FullTimeEmployee);
            int partTime = employees.Count(e => e is PartTimeEmployee);

            Console.WriteLine($"Total Employees: {total}");
            Console.WriteLine($"Full-Time: {fullTime}");
            Console.WriteLine($"Part-Time: {partTime}");

            if (total == 0) return;

            var avgYears = employees.Average(e => (DateTime.Now - e.HireDate).TotalDays / 365);
            Console.WriteLine($"Average Tenure: {avgYears:F1} years");
        }

        // ------------------------------
        // Search (case-insensitive partial match)
        // ------------------------------
        public List<Employee> SearchEmployeesByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new();

            return employees
                .Where(e => e.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Name)
                .ToList();
        }
    }
}
