using System;

namespace EmployeeTimeTracker.Models
{
    /// <summary>
    /// Full-time employee model with monthly salary and overtime rate.
    /// Includes validation for realistic input values.
    /// </summary>
    public class FullTimeEmployee : Employee
    {
        // To prevent absurd numbers like 999 trillion during user input
        private const decimal MAX_ALLOWED_VALUE = 1_000_000_000m;

        public decimal MonthlySalary { get; private set; }
        public decimal OvertimeRate { get; private set; }

        public FullTimeEmployee(
            string employeeId,
            string name,
            string email,
            DateTime hireDate,
            decimal monthlySalary,
            decimal overtimeRate)
            : base(employeeId, name, email, hireDate)
        {
            MonthlySalary = ValidateMoney(monthlySalary, nameof(monthlySalary));
            OvertimeRate = ValidateMoney(overtimeRate, nameof(overtimeRate));
        }

        /// <summary>
        /// Prevents negative or unrealistic money values.
        /// </summary>
        private static decimal ValidateMoney(decimal value, string field)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(field, $"{field} cannot be negative.");

            if (value > MAX_ALLOWED_VALUE)
                throw new ArgumentOutOfRangeException(field, $"{field} exceeds the allowed limit.");

            return value;
        }

        /// <summary>
        /// Full-time employees always return monthly salary.
        /// Hours worked is ignored unless overtime is implemented later.
        /// </summary>
        public override decimal CalculatePay(double hoursWorked)
        {
            return MonthlySalary;
        }

        /// <summary>
        /// Returns monthly salary without overtime.
        /// </summary>
        public decimal CalculatePay()
        {
            return MonthlySalary;
        }

        /// <summary>
        /// Prorates salary across a date range (inclusive). 
        /// Handles multi-month periods accurately.
        /// </summary>
        public decimal CalculatePay(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End date cannot be before start date.");

            start = start.Date;
            end = end.Date;

            decimal total = 0m;

            DateTime cursor = new(start.Year, start.Month, 1);
            DateTime lastMonth = new(end.Year, end.Month, 1);

            while (cursor <= lastMonth)
            {
                int year = cursor.Year;
                int month = cursor.Month;
                int daysInMonth = DateTime.DaysInMonth(year, month);

                DateTime monthStart = new(year, month, 1);
                DateTime monthEnd = new(year, month, daysInMonth);

                DateTime overlapStart = (start > monthStart) ? start : monthStart;
                DateTime overlapEnd = (end < monthEnd) ? end : monthEnd;

                if (overlapEnd >= overlapStart)
                {
                    double overlapDays = (overlapEnd - overlapStart).TotalDays + 1;
                    decimal fraction = (decimal)overlapDays / daysInMonth;

                    total += MonthlySalary * fraction;
                }

                cursor = cursor.AddMonths(1);
            }

            return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        public override void DisplayInfo()
        {
            base.DisplayInfo();
            Console.WriteLine("Type: Full-Time");
            Console.WriteLine($"Monthly Salary: {MonthlySalary:C}");
            Console.WriteLine($"Overtime Rate: {OvertimeRate:C}/hour");
        }
    }
}
