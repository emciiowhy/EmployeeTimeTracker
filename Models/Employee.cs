using System;
using System.Text.RegularExpressions;
using EmployeeTimeTracker.Utilities;

namespace EmployeeTimeTracker.Models
{
    public abstract class Employee
    {
        private string _employeeId = string.Empty;
        private string _name = string.Empty;
        private string _email = string.Empty;
        private DateTime _hireDate;

        // Allowed characters for Employee ID → A–Z a–z 0–9 - _
        private static readonly Regex EmployeeIdRegex =
            new(@"^[A-Za-z0-9\-_]+$", RegexOptions.Compiled);

        // Name must contain at least one alphabet letter
        private static readonly Regex NameRegex =
            new(@"[A-Za-z]", RegexOptions.Compiled);

        public string EmployeeId
        {
            get => _employeeId;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Employee ID cannot be empty.");

                if (!EmployeeIdRegex.IsMatch(value))
                    throw new ArgumentException("Employee ID can only contain letters, numbers, '-' or '_'.");

                _employeeId = value.Trim();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be empty.");

                if (!NameRegex.IsMatch(value))
                    throw new ArgumentException("Name must contain at least one letter.");

                _name = value.Trim();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email cannot be empty.");

                if (!Validators.IsValidEmail(value))
                    throw new ArgumentException("Invalid email format.");

                _email = value.Trim();
            }
        }

        public DateTime HireDate
        {
            get => _hireDate;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("Hire date cannot be in the future.");

                _hireDate = value;
            }
        }

        protected Employee(string employeeId, string name, string email, DateTime hireDate)
        {
            EmployeeId = employeeId;
            Name = name;
            Email = email;
            HireDate = hireDate;
        }

        public abstract decimal CalculatePay(double hoursWorked);

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"ID: {EmployeeId}");
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Email: {Email}");
            Console.WriteLine($"Hire Date: {HireDate:MM/dd/yyyy}");
        }

        public string GetEmployeeType() => GetType().Name;
    }
}
