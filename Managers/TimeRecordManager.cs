using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmployeeTimeTracker.Models;

namespace EmployeeTimeTracker.Managers
{
    public class TimeRecordManager
    {
        private readonly Dictionary<string, List<TimeRecord>> recordsByEmployee;
        private readonly List<TimeRecord> allRecords;

        private const string RecordsFilePath = "timerecords.txt";
        private int recordCounter = 1;

        public delegate void ClockEventHandler(string employeeId, DateTime time, string action);

        // init to empty delegate so invocation is safe
        public event ClockEventHandler OnClockEvent = delegate { };

        public TimeRecordManager()
        {
            recordsByEmployee = new Dictionary<string, List<TimeRecord>>();
            allRecords = new List<TimeRecord>();
        }

        public void ClockIn(string employeeId, string notes = "")
        {
            notes ??= "";

            try
            {
                var activeRecord = allRecords.FirstOrDefault(r => r.EmployeeId == employeeId && r.IsActive);
                if (activeRecord != null) throw new InvalidOperationException($"Employee {employeeId} is already clocked in.");

                string recordId = $"TR{recordCounter++:D4}";
                var record = new TimeRecord(recordId, employeeId, DateTime.Now, notes);

                allRecords.Add(record);

                if (!recordsByEmployee.ContainsKey(employeeId))
                    recordsByEmployee[employeeId] = new List<TimeRecord>();
                recordsByEmployee[employeeId].Add(record);

                OnClockEvent(employeeId, DateTime.Now, "Clock In");
                Console.WriteLine($"Employee {employeeId} clocked in at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Clock-in error: {ex.Message}");
                throw;
            }
        }

        public void ClockOut(string employeeId)
        {
            try
            {
                var activeRecord = allRecords.FirstOrDefault(r => r.EmployeeId == employeeId && r.IsActive);
                if (activeRecord == null) throw new InvalidOperationException($"Employee {employeeId} has no active clock-in.");

                activeRecord.ClockOutNow();
                OnClockEvent(employeeId, DateTime.Now, "Clock Out");
                Console.WriteLine($"Employee {employeeId} clocked out. Hours worked: {activeRecord.HoursWorked:F2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Clock-out error: {ex.Message}");
                throw;
            }
        }

        public List<TimeRecord> GetEmployeeRecords(string employeeId)
        {
            if (recordsByEmployee.ContainsKey(employeeId)) return new List<TimeRecord>(recordsByEmployee[employeeId]);
            return new List<TimeRecord>();
        }

        // Convenience method used by tests and some earlier code
        public List<TimeRecord> GetRecordsForEmployee(string employeeId) => GetEmployeeRecords(employeeId);

        public double CalculateTotalHours(string employeeId, DateTime startDate, DateTime endDate)
        {
            if (!recordsByEmployee.ContainsKey(employeeId)) return 0;
            return recordsByEmployee[employeeId]
                    .Where(r => r.ClockIn >= startDate && r.ClockIn <= endDate && r.ClockOut.HasValue)
                    .Sum(r => r.HoursWorked);
        }

        public void DisplayEmployeeRecords(string employeeId)
        {
            var records = GetEmployeeRecords(employeeId);
            if (records.Count == 0) { Console.WriteLine($"No records found for employee {employeeId}"); return; }

            Console.WriteLine($"\n=== TIME RECORDS FOR {employeeId} ===");
            records.ForEach(r => r.DisplayRecord());

            double totalHours = records.Where(r => r.ClockOut.HasValue).Sum(r => r.HoursWorked);
            Console.WriteLine($"\nTotal Hours: {totalHours:F2}");
        }

        public void SaveToFile()
        {
            try
            {
                using StreamWriter writer = new StreamWriter(RecordsFilePath);
                foreach (var record in allRecords)
                {
                    string clockOut = record.ClockOut.HasValue ? record.ClockOut.Value.ToString("yyyy-MM-dd HH:mm:ss") : "NULL";
                    writer.WriteLine($"{record.RecordId}|{record.EmployeeId}|{record.ClockIn:yyyy-MM-dd HH:mm:ss}|{clockOut}|{record.Notes}");
                }
                Console.WriteLine("Time records saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving records: {ex.Message}");
            }
        }

        public void LoadFromFile()
        {
            try
            {
                if (!File.Exists(RecordsFilePath)) { Console.WriteLine("No saved time records found."); return; }

                allRecords.Clear();
                recordsByEmployee.Clear();

                using StreamReader reader = new StreamReader(RecordsFilePath);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length < 5) continue;

                    string recordId = parts[0];
                    string employeeId = parts[1];
                    DateTime clockIn = DateTime.Parse(parts[2]);
                    DateTime? clockOut = parts[3] == "NULL" ? null : DateTime.Parse(parts[3]);
                    string notes = parts[4] ?? "";

                    var record = new TimeRecord(recordId, employeeId, clockIn, notes);
                    if (clockOut.HasValue) record.ClockOut = clockOut;

                    allRecords.Add(record);
                    if (!recordsByEmployee.ContainsKey(employeeId)) recordsByEmployee[employeeId] = new List<TimeRecord>();
                    recordsByEmployee[employeeId].Add(record);

                    if (recordId.StartsWith("TR"))
                    {
                        int num = int.Parse(recordId.Substring(2));
                        if (num >= recordCounter) recordCounter = num + 1;
                    }
                }

                Console.WriteLine($"Loaded {allRecords.Count} time records from file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading records: {ex.Message}");
            }
        }
    }
}
