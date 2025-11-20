using System;

namespace EmployeeTimeTracker.Models
{
    public class PartTimeEmployee : Employee
    {
        // Prevent unrealistic hourly rates (ex: 500 billion per hour)
        private const decimal MAX_ALLOWED_RATE = 1_000_000_000m;

        private decimal _hourlyRate;

        public decimal HourlyRate
        {
            get => _hourlyRate;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Hourly rate cannot be negative.");

                if (value > MAX_ALLOWED_RATE)
                    throw new ArgumentOutOfRangeException(nameof(value), "Hourly rate exceeds allowed limit.");

                _hourlyRate = value;
            }
        }

        public PartTimeEmployee(
            string employeeId,
            string name,
            string email,
            DateTime hireDate,
            decimal hourlyRate)
            : base(employeeId, name, email, hireDate)
        {
            HourlyRate = hourlyRate; // Validation happens in setter
        }

        /// <summary>
        /// Pay = HourlyRate × hoursWorked
        /// Includes validation for negative, NaN, or infinity values.
        /// </summary>
        public override decimal CalculatePay(double hoursWorked)
        {
            if (hoursWorked < 0)
                throw new ArgumentException("Hours worked cannot be negative.", nameof(hoursWorked));

            if (double.IsNaN(hoursWorked) || double.IsInfinity(hoursWorked))
                throw new ArgumentException("Invalid numeric value for hoursWorked.", nameof(hoursWorked));

            return HourlyRate * (decimal)hoursWorked;
        }

        public override void DisplayInfo()
        {
            base.DisplayInfo();
            Console.WriteLine("Type: Part-Time");
            Console.WriteLine($"Hourly Rate: {HourlyRate:C}/hour");
        }
    }
}
