using System;

namespace EmployeeTimeTracker.Models
{
    public class TimeRecord
    {
        private const double MAX_SHIFT_HOURS = 24 * 3; // 3 days; prevent corrupted or insane shifts

        public string RecordId { get; set; }
        public string EmployeeId { get; set; }
        public DateTime ClockIn { get; private set; }
        public DateTime? ClockOut { get; private set; }
        public string Notes { get; set; }

        public bool IsActive => !ClockOut.HasValue;

        public double HoursWorked
        {
            get
            {
                if (!ClockOut.HasValue) return 0;

                var total = (ClockOut.Value - ClockIn).TotalHours;

                if (total < 0) return 0; // corrupted clock or timezone shift

                if (total > MAX_SHIFT_HOURS)
                    return MAX_SHIFT_HOURS; // cap insane values

                return total;
            }
        }

        public TimeRecord(string recordId, string employeeId, DateTime clockIn, string notes = "")
        {
            if (clockIn > DateTime.Now.AddMinutes(1))
                throw new ArgumentException("Clock-in cannot be in the future.");

            RecordId = recordId;
            EmployeeId = employeeId;
            ClockIn = clockIn;
            ClockOut = null;
            Notes = notes ?? "";
        }

        // -------------------------------------------------
        // Safe Clock-Out (NOW)
        // -------------------------------------------------
        public void ClockOutNow()
        {
            if (ClockOut.HasValue)
                throw new InvalidOperationException("This record is already clocked out.");

            DateTime now = DateTime.Now;

            if (now < ClockIn)
                throw new InvalidOperationException("Clock-out cannot be earlier than clock-in.");

            ClockOut = now;
        }

        // -------------------------------------------------
        // Clock-out at CUSTOM time (for reports/testing)
        // -------------------------------------------------
        public void ClockOutAt(DateTime timestamp)
        {
            if (ClockOut.HasValue)
                throw new InvalidOperationException("This record has already been clocked out.");

            if (timestamp < ClockIn)
                throw new ArgumentException("Clock-out time cannot be before clock-in.");

            if (timestamp > DateTime.Now.AddMinutes(1))
                throw new ArgumentException("Clock-out cannot be in the future.");

            ClockOut = timestamp;
        }

        // -------------------------------------------------
        // Display with protection against nulls
        // -------------------------------------------------
        public void DisplayRecord()
        {
            Console.WriteLine($"Record ID: {RecordId}");
            Console.WriteLine($"Clock In: {ClockIn}");
            Console.WriteLine($"Clock Out: {(ClockOut.HasValue ? ClockOut.Value.ToString() : "Still working")}");

            Console.WriteLine($"Hours: {HoursWorked:F2}");

            if (!string.IsNullOrWhiteSpace(Notes))
            {
                Console.WriteLine($"Notes: {Notes}");
            }
        }
    }
}
