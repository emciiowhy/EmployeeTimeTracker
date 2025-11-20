using System;

namespace EmployeeTimeTracker.Models
{
    /// <summary>
    /// Represents a scheduled work shift for an employee.
    /// A Shift is not a time record—it's a planned schedule.
    /// Actual attendance is stored in TimeRecord.
    /// </summary>
    public class Shift
    {
        public int ShiftId { get; set; }
        public int EmployeeId { get; set; }

        /// <summary>
        /// Planned start of shift.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Planned end of shift.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// True if the shift spans past midnight.
        /// (Example: Start = 10PM, End = 6AM)
        /// </summary>
        public bool IsOvernight =>
            EndTime.Date > StartTime.Date;

        /// <summary>
        /// Total hours scheduled for the shift.
        /// Automatically adjusted for overnight shifts.
        /// </summary>
        public double TotalHours =>
            (EndTime - StartTime).TotalHours;

        public Shift() { }

        public Shift(int shiftId, int employeeId, DateTime startTime, DateTime endTime)
        {
            ShiftId = shiftId;
            EmployeeId = employeeId;
            StartTime = startTime;
            EndTime = endTime;
        }

        // -------------------------------------------------------------------------
        // Serialization (Text File Format)
        // Aligns with your current FileHandler and simple text DB.
        // Format: ShiftId|EmployeeId|Start|End
        // -------------------------------------------------------------------------

        public override string ToString()
        {
            return $"{ShiftId}|{EmployeeId}|{StartTime:o}|{EndTime:o}";
        }

        public static Shift FromString(string line)
        {
            var parts = line.Split('|');
            if (parts.Length != 4) throw new FormatException("Invalid shift record.");

            return new Shift
            {
                ShiftId = int.Parse(parts[0]),
                EmployeeId = int.Parse(parts[1]),
                StartTime = DateTime.Parse(parts[2]),
                EndTime = DateTime.Parse(parts[3])
            };
        }
    }
}
