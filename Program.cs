using System;
using System.Globalization;
using System.Linq;
using EmployeeTimeTracker.Managers;
using EmployeeTimeTracker.Models;
using EmployeeTimeTracker.Utilities;

namespace EmployeeTimeTracker
{
    internal static class Program
    {
        // -------------------------
        // Configurable sanity limits
        // -------------------------
        private const decimal MAX_MONTHLY_SALARY = 1_000_000m;
        private const decimal MAX_HOURLY_RATE = 10_000m;
        private const decimal MAX_OVERTIME_RATE = 10_000m;

        // Managers (keep singletons here)
        private static readonly EmployeeManager employeeManager = new();
        private static readonly TimeRecordManager timeManager = new();

        // Entry point
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            AppStartup();

            try
            {
                RunMainLoop();
            }
            catch (Exception ex)
            {
                // Last-resort catcher to avoid crashing; log helpful info
                Console.WriteLine($"\n[FATAL] Unexpected error: {ex.Message}");
            }
            finally
            {
                // On exit, ask user to save (Mode C: confirm)
                Console.WriteLine("\nExiting. Save data before leaving? (Y/n)");
                if (AskYesNo(true))
                {
                    employeeManager.SaveToFile();
                    timeManager.SaveToFile();
                }
            }
        }

        // -------------------------
        // Startup: load data & hooks
        // -------------------------
        private static void AppStartup()
        {
            Console.WriteLine("Loading saved data...");
            employeeManager.LoadFromFile();
            timeManager.LoadFromFile();

            // Keep UI notified of employee events
            employeeManager.OnEmployeeChanged += (msg) => Console.WriteLine($"[NOTIFICATION] {msg}");

            // Keep UI notified of time events (clock in/out)
            timeManager.OnClockEvent += (empId, time, action) =>
                Console.WriteLine($"[CLOCK EVENT] {empId} - {action} at {time:HH:mm:ss}");
        }

        // -------------------------
        // Main loop
        // -------------------------
        private static void RunMainLoop()
        {
            while (true)
            {
                PrintHeader();
                Console.WriteLine("[1] Employee Management");
                Console.WriteLine("[2] Time Tracking");
                Console.WriteLine("[3] Reports");
                Console.WriteLine("[4] Save & Exit");
                Console.Write("\nSelect option: ");

                int mainOpt = ReadIntInRange(1, 4, allowZero: false);
                Console.WriteLine();

                switch (mainOpt)
                {
                    case 1: EmployeeMenu(); break;
                    case 2: TimeMenu(); break;
                    case 3: ReportMenu(); break;
                    case 4:
                        Console.WriteLine("Save data and exit? (Y/n)");
                        if (AskYesNo(true))
                        {
                            employeeManager.SaveToFile();
                            timeManager.SaveToFile();
                            Console.WriteLine("Data saved. Goodbye!");
                        }
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        // =========================
        // Employee management menu
        // =========================
        private static void EmployeeMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- EMPLOYEE MANAGEMENT ---");
                Console.WriteLine("[1] Add Full-Time Employee");
                Console.WriteLine("[2] Add Part-Time Employee");
                Console.WriteLine("[3] Remove Employee");
                Console.WriteLine("[4] View All Employees");
                Console.WriteLine("[5] Find Employee");
                Console.WriteLine("[6] Search Employee by Name");
                Console.WriteLine("[0] Back to Main Menu");
                Console.Write("\nSelect: ");

                int opt = ReadIntInRange(0, 6, allowZero: true);
                Console.WriteLine();

                switch (opt)
                {
                    case 1: AddFullTimeEmployee(); break;
                    case 2: AddPartTimeEmployee(); break;
                    case 3: RemoveEmployee(); break;
                    case 4: employeeManager.DisplayAllEmployees(); break;
                    case 5: FindEmployee(); break;
                    case 6: SearchEmployeeByName(); break;
                    case 0: return;
                }
            }
        }

        // -------------------------
        // Add Full-Time Employee
        // (Mode C: re-prompt only the invalid field)
        // -------------------------
        private static void AddFullTimeEmployee()
        {
            Console.WriteLine("\n--- Add Full-Time Employee ---");

            // Employee ID (critical): validate format & uniqueness
            string id = ReadUntilValid(
                prompt: "Employee ID: ",
                validate: s =>
                {
                    if (!Validators.IsValidEmployeeIdFormat(s))
                        return "Invalid Employee ID. Allowed: letters, digits, '-' and '_'.";
                    if (employeeManager.IdExists(s))
                        return "Employee ID already exists.";
                    return null;
                });

            // Name (critical): must be letters + spaces
            string name = ReadUntilValid(
                prompt: "Name: ",
                validate: s =>
                {
                    if (!Validators.IsValidName(s))
                        return "Invalid name. Must contain only letters and spaces.";
                    return null;
                });

            // Email (critical): format and uniqueness
            string email = ReadUntilValid(
                prompt: "Email: ",
                validate: s =>
                {
                    if (!Validators.IsValidEmail(s))
                        return "Invalid email format.";
                    if (employeeManager.EmailExists(s))
                        return "Email already used by another employee.";
                    return null;
                });

            // Hire date (non-critical — optional)
            Console.Write("Hire date (press Enter for today) [flexible formats allowed]: ");
            DateTime hireDate = ReadOptionalDateSmart(DateTime.Now);

            // Monthly salary (critical): numeric and within sane bounds
            decimal monthly = ReadDecimalUntilValid(
                prompt: "Monthly Salary: ",
                min: 0m, max: MAX_MONTHLY_SALARY,
                invalidMessage: $"Enter a decimal between 0 and {MAX_MONTHLY_SALARY:N0}.");

            // Overtime rate (critical)
            decimal overtime = ReadDecimalUntilValid(
                prompt: "Overtime Rate (per hour): ",
                min: 0m, max: MAX_OVERTIME_RATE,
                invalidMessage: $"Enter a decimal between 0 and {MAX_OVERTIME_RATE:N0}.");

            var emp = new FullTimeEmployee(id, name, email, hireDate, monthly, overtime);

            Console.WriteLine("\nConfirm adding employee:");
            emp.DisplayInfo();
            Console.WriteLine("\nConfirm? (Y/n)");
            if (!AskYesNo(true)) { Console.WriteLine("Cancelled."); return; }

            try
            {
                employeeManager.AddEmployee(emp);
                Console.WriteLine("Full-time employee added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding employee: {ex.Message}");
            }
        }

        // -------------------------
        // Add Part-Time Employee
        // -------------------------
        private static void AddPartTimeEmployee()
        {
            Console.WriteLine("\n--- Add Part-Time Employee ---");

            string id = ReadUntilValid(
                prompt: "Employee ID: ",
                validate: s =>
                {
                    if (!Validators.IsValidEmployeeIdFormat(s))
                        return "Invalid Employee ID. Allowed: letters, digits, '-' and '_'.";
                    if (employeeManager.IdExists(s))
                        return "Employee ID already exists.";
                    return null;
                });

            string name = ReadUntilValid(
                prompt: "Name: ",
                validate: s =>
                {
                    if (!Validators.IsValidName(s))
                        return "Invalid name. Must contain only letters and spaces.";
                    return null;
                });

            string email = ReadUntilValid(
                prompt: "Email: ",
                validate: s =>
                {
                    if (!Validators.IsValidEmail(s))
                        return "Invalid email format.";
                    if (employeeManager.EmailExists(s))
                        return "Email already used by another employee.";
                    return null;
                });

            Console.Write("Hire date (press Enter for today) [flexible formats allowed]: ");
            DateTime hireDate = ReadOptionalDateSmart(DateTime.Now);

            decimal rate = ReadDecimalUntilValid(
                prompt: "Hourly Rate: ",
                min: 0m, max: MAX_HOURLY_RATE,
                invalidMessage: $"Enter a decimal between 0 and {MAX_HOURLY_RATE:N0}.");

            var emp = new PartTimeEmployee(id, name, email, hireDate, rate);

            Console.WriteLine("\nConfirm adding employee:");
            emp.DisplayInfo();
            Console.WriteLine("\nConfirm? (Y/n)");
            if (!AskYesNo(true)) { Console.WriteLine("Cancelled."); return; }

            try
            {
                employeeManager.AddEmployee(emp);
                Console.WriteLine("Part-time employee added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding employee: {ex.Message}");
            }
        }

        // -------------------------
        // Remove / Find / Search
        // -------------------------
        private static void RemoveEmployee()
        {
            Console.Write("Enter Employee ID to remove: ");
            string id = ReadNonEmptyTrimmed();
            var emp = employeeManager.FindEmployeeById(id);
            if (emp == null) { Console.WriteLine("Employee not found."); return; }

            Console.WriteLine("Found:");
            emp.DisplayInfo();
            Console.WriteLine("\nAre you sure you want to delete this employee? This action cannot be undone. (y/N)");
            if (!AskYesNo(false)) { Console.WriteLine("Aborted."); return; }

            if (employeeManager.RemoveEmployee(id)) Console.WriteLine("Employee removed.");
            else Console.WriteLine("Removal failed.");
        }

        private static void FindEmployee()
        {
            Console.Write("Enter Employee ID: ");
            string id = ReadNonEmptyTrimmed();
            var emp = employeeManager.FindEmployeeById(id);
            if (emp == null) Console.WriteLine("Employee not found.");
            else emp.DisplayInfo();
        }

        private static void SearchEmployeeByName()
        {
            Console.WriteLine("\n--- Employee Search ---");
            Console.Write("Enter employee name (full or partial): ");
            string q = ReadNonEmptyTrimmed();
            var results = employeeManager.SearchEmployeesByName(q);
            if (results.Count == 0) { Console.WriteLine("No employees found."); return; }

            foreach (var r in results) r.DisplayInfo();
        }

        // =========================
        // Time tracking menu
        // =========================
        private static void TimeMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- TIME TRACKING ---");
                Console.WriteLine("[1] Clock In");
                Console.WriteLine("[2] Clock Out");
                Console.WriteLine("[3] View Employee Time Records");
                Console.WriteLine("[0] Back");
                Console.Write("\nSelect: ");

                int opt = ReadIntInRange(0, 3, allowZero: true);
                Console.WriteLine();

                switch (opt)
                {
                    case 1: ClockInFlow(); break;
                    case 2: ClockOutFlow(); break;
                    case 3: ViewRecords(); break;
                    case 0: return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
        }

        // Clock in: validate employee exists before accepting notes (good UX)
        private static void ClockInFlow()
        {
            Console.Write("Employee ID: ");
            string id = ReadNonEmptyTrimmed();
            var emp = employeeManager.FindEmployeeById(id);
            if (emp == null) { Console.WriteLine("Employee not found."); return; }

            Console.Write("Notes (optional): ");
            string notes = Console.ReadLine() ?? "";

            try
            {
                timeManager.ClockIn(id, notes);
                Console.WriteLine($"Employee {id} clocked in at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Clock-in error: {ex.Message}");
            }
        }

        // Clock out: validate employee exists and rely on TimeRecordManager for errors
        private static void ClockOutFlow()
        {
            Console.Write("Employee ID: ");
            string id = ReadNonEmptyTrimmed();
            var emp = employeeManager.FindEmployeeById(id);
            if (emp == null) { Console.WriteLine("Employee not found."); return; }

            try
            {
                timeManager.ClockOut(id);
                Console.WriteLine($"Employee {id} clocked out at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Clock-out error: {ex.Message}");
            }
        }

        private static void ViewRecords()
        {
            Console.Write("Employee ID: ");
            string id = ReadNonEmptyTrimmed();
            var emp = employeeManager.FindEmployeeById(id);
            if (emp == null) { Console.WriteLine("Employee not found."); return; }

            timeManager.DisplayEmployeeRecords(id);
        }

        // =========================
        // Reports menu
        // =========================
        private static void ReportMenu()
        {
            while (true)
            {
                Console.WriteLine("\n--- REPORTS ---");
                Console.WriteLine("[1] Employee Summary Report");
                Console.WriteLine("[2] Calculate Pay for Employee");
                Console.WriteLine("[3] Monthly Hours Report");
                Console.WriteLine("[0] Back");
                Console.Write("\nSelect: ");

                int opt = ReadIntInRange(0, 3, allowZero: true);
                Console.WriteLine();

                switch (opt)
                {
                    case 1: employeeManager.GenerateSummaryReport(); break;
                    case 2: CalculatePayReport(); break;
                    case 3: MonthlyHoursReport(); break;
                    case 0: return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
        }

        // Calculate pay: uses FullTimeEmployee.CalculatePay(start,end) (prorated)
        // and PartTimeEmployee.CalculatePay(hours) for hourly staff
        private static void CalculatePayReport()
        {
            Console.Write("Employee ID: ");
            string id = ReadNonEmptyTrimmed();
            var emp = employeeManager.FindEmployeeById(id);
            if (emp == null) { Console.WriteLine("Employee not found."); return; }

            Console.Write("Enter start date (flexible formats allowed): ");
            DateTime start = ReadDateSmart();

            Console.Write("Enter end date (flexible formats allowed): ");
            DateTime end = ReadDateSmart();

            if (end < start) { Console.WriteLine("End date must be after start date."); return; }

            decimal totalPay = 0m;
            double totalHours = 0;

            if (emp is FullTimeEmployee ft)
            {
                try
                {
                    totalPay = ft.CalculatePay(start, end);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calculating pay for full-time employee: {ex.Message}");
                    return;
                }
            }
            else if (emp is PartTimeEmployee pt)
            {
                totalHours = timeManager.CalculateTotalHours(id, start, end);
                try
                {
                    totalPay = pt.CalculatePay(totalHours);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calculating pay for part-time employee: {ex.Message}");
                    return;
                }
            }

            Console.WriteLine("\n--- PAY CALCULATION ---");
            Console.WriteLine($"Employee: {emp.Name}");
            Console.WriteLine($"Period: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");
            Console.WriteLine($"Total Hours: {totalHours:F2}");
            Console.WriteLine($"Total Pay: {totalPay:C}");
        }

        private static void MonthlyHoursReport()
        {
            var today = DateTime.Now;
            var firstOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);
            Console.WriteLine($"\n--- MONTHLY HOURS REPORT ---");
            Console.WriteLine($"Period: {firstOfMonth:yyyy-MM-dd} to {lastOfMonth:yyyy-MM-dd}\n");

            foreach (var emp in employeeManager.GetAllEmployees())
            {
                double hours = timeManager.CalculateTotalHours(emp.EmployeeId, firstOfMonth, lastOfMonth);
                Console.WriteLine($"{emp.EmployeeId} - {emp.Name}: {hours:F2} hours");
            }
        }

        // =========================
        // Helper Input Methods (Mode C)
        // - Re-prompt only the invalid field
        // - Date smart parser accepts date + optional time
        // =========================

        /// <summary>
        /// Reads an int and enforces range. If allowZero is true, 0 is valid (used for menu back).
        /// This method will repeatedly prompt until a valid integer in the allowed range is entered.
        /// </summary>
        private static int ReadIntInRange(int min, int max, bool allowZero)
        {
            while (true)
            {
                var s = Console.ReadLine() ?? "";
                if (int.TryParse(s.Trim(), out int val))
                {
                    if (val == 0 && allowZero) return 0;
                    if (val >= min && val <= max) return val;
                }
                Console.Write("Invalid option. Try again: ");
            }
        }

        /// <summary>
        /// Read non-empty string, trims whitespace. Re-prompts until non-empty.
        /// </summary>
        private static string ReadNonEmptyTrimmed()
        {
            while (true)
            {
                var s = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.Write("Input required. Try again: ");
            }
        }

        /// <summary>
        /// Read email with Validator.IsValidEmail. Re-prompts only the email field (Mode C).
        /// </summary>
        private static string ReadValidEmailField()
        {
            while (true)
            {
                var s = Console.ReadLine() ?? "";
                s = s.Trim();
                if (Validators.IsValidEmail(s)) return s;
                Console.Write("? Invalid email format. Try again: ");
            }
        }

        /// <summary>
        /// Smart date parser: accepts many common formats, and also accepts time if provided.
        /// (Option 1 UX + user requested accepting time as well.)
        /// Re-prompts only this date field.
        /// </summary>
        private static DateTime ReadDateSmart()
        {
            while (true)
            {
                var raw = Console.ReadLine() ?? "";
                raw = raw.Trim();
                if (TryParseFlexibleDateTime(raw, out DateTime dt)) return dt;
                Console.Write("? Invalid date/time. Try again (e.g. 11/21/2025 or 2025-11-21 14:30): ");
            }
        }

        /// <summary>
        /// For optional hire date where empty input means defaultValue.
        /// </summary>
        private static DateTime ReadOptionalDateSmart(DateTime defaultValue)
        {
            var raw = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(raw)) return defaultValue.Date;

            raw = raw.Trim();
            if (TryParseFlexibleDateTime(raw, out DateTime dt)) return dt.Date;

            // Re-prompt only this field (Mode C)
            Console.WriteLine("? Invalid date. Try again.");
            return ReadOptionalDateSmart(defaultValue);
        }

        /// <summary>
        /// Attempt to parse a wide range of date/time formats.
        /// Returns true if parsed. Accepts time as well if provided.
        /// Uses invariant culture and a list of common patterns; also falls back to DateTime.TryParse.
        /// </summary>
        private static bool TryParseFlexibleDateTime(string input, out DateTime result)
        {
            // Try common explicit patterns first (date + optional time)
            string[] patterns =
            {
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-dd H:mm",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm",
                "M/d/yyyy H:mm",
                "MM/dd/yyyy",
                "M/d/yyyy",
                "dd/MM/yyyy",
                "d/M/yyyy",
                "yyyy/MM/dd",
                "dd MMM yyyy",
                "d MMM yyyy",
                "MMM dd yyyy",
                "MMMM dd, yyyy",
                "yyyy-MM-ddTHH:mm",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "M/d/yyyy h:mm tt",
                "MM/dd/yyyy h:mm tt"
            };

            // 1) Try exact patterns with invariant culture
            foreach (var p in patterns)
            {
                if (DateTime.TryParseExact(input, p, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out result))
                {
                    return true;
                }
            }

            // 2) Fallback to general parse (accepts many localized forms)
            if (DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out result))
                return true;

            if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out result))
                return true;

            result = default;
            return false;
        }

        /// <summary>
        /// Reusable helper: prompt until validate returns null (valid) or a string message (invalid).
        /// Returns the successful trimmed input.
        /// This implements Mode C: only re-prompt the failed field.
        /// </summary>
        private static string ReadUntilValid(string prompt, Func<string, string?> validate)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = (Console.ReadLine() ?? "").Trim();

                string? problem = validate(s);
                if (problem == null) return s;

                Console.WriteLine($"? {problem}"); // show reason
                // re-prompt only this field (Mode C)
            }
        }

        /// <summary>
        /// Read a decimal with bounds and re-prompt until valid.
        /// </summary>
        private static decimal ReadDecimalUntilValid(string prompt, decimal min, decimal max, string invalidMessage)
        {
            while (true)
            {
                Console.Write(prompt);
                var raw = Console.ReadLine() ?? "";
                raw = raw.Trim();
                if (decimal.TryParse(raw, out decimal d) && d >= min && d <= max)
                    return d;

                Console.WriteLine($"? {invalidMessage}");
                // re-prompt only this field (Mode C)
            }
        }

        /// <summary>
        /// Simplified wrapper used earlier for menus.
        /// </summary>
        private static string ReadValidEmail()
        {
            // Reuse the read-until-valid pattern for email in older code paths
            return ReadUntilValid("Email: ", s =>
            {
                if (!Validators.IsValidEmail(s)) return "Invalid email format.";
                return null;
            });
        }

        /// <summary>
        /// Minimal yes/no helper: returns true for yes, false for no.
        /// Default is used when input empty.
        /// </summary>
        private static bool AskYesNo(bool defaultYes)
        {
            var s = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(s)) return defaultYes;
            s = s.Trim().ToLowerInvariant();
            if (s == "y" || s == "yes") return true;
            if (s == "n" || s == "no") return false;
            return defaultYes;
        }

        // -------------------------
        // Utility small helpers
        // -------------------------
        private static string ReadNonEmptyTrimmedPrompt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.WriteLine("Input required. Try again.");
            }
        }

        private static string ReadNonEmptyTrimmed()
        {
            while (true)
            {
                var s = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.Write("Input required. Try again: ");
            }
        }

        // -------------------------
        // Pretty header
        // -------------------------
        private static void PrintHeader()
        {
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║   EMPLOYEE TIME TRACKER SYSTEM         ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
        }
    }
}
