using System;

namespace EmployeeTimeTracker.Delegates
{
    /// <summary>
    /// Contains delegate types and event argument models
    /// for employee and time record notifications.
    /// </summary>
    public static class NotificationDelegates
    {
        // -------------------------------------------------------
        // EMPLOYEE-RELATED DELEGATES
        // -------------------------------------------------------

        /// <summary>
        /// Triggered when a new employee is added.
        /// </summary>
        public delegate void EmployeeAddedHandler(object sender, EmployeeEventArgs e);

        /// <summary>
        /// Triggered when an employee record is updated.
        /// </summary>
        public delegate void EmployeeUpdatedHandler(object sender, EmployeeEventArgs e);

        /// <summary>
        /// Triggered when an employee is removed.
        /// </summary>
        public delegate void EmployeeDeletedHandler(object sender, EmployeeEventArgs e);

        // -------------------------------------------------------
        // TIME RECORD DELEGATES
        // -------------------------------------------------------

        /// <summary>
        /// Triggered when an employee clocks in.
        /// </summary>
        public delegate void ClockInHandler(object sender, TimeRecordEventArgs e);

        /// <summary>
        /// Triggered when an employee clocks out.
        /// </summary>
        public delegate void ClockOutHandler(object sender, TimeRecordEventArgs e);

        // -------------------------------------------------------
        // EVENT ARGUMENT CLASSES
        // -------------------------------------------------------

        /// <summary>
        /// Used for passing employee-related data to event subscribers.
        /// </summary>
        public class EmployeeEventArgs : EventArgs
        {
            public int EmployeeId { get; }
            public string Name { get; }
            public string Action { get; }

            public EmployeeEventArgs(int employeeId, string name, string action)
            {
                EmployeeId = employeeId;
                Name = name;
                Action = action;
            }
        }

        /// <summary>
        /// Used for passing time record data to event subscribers.
        /// </summary>
        public class TimeRecordEventArgs : EventArgs
        {
            public int EmployeeId { get; }
            public DateTime Timestamp { get; }
            public string Action { get; }

            public TimeRecordEventArgs(int employeeId, DateTime timestamp, string action)
            {
                EmployeeId = employeeId;
                Timestamp = timestamp;
                Action = action;
            }
        }
    }
}
