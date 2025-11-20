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
        // Limits for sanity checks
        private const decimal MAX_MONTHLY_SALARY = 1_000_000m;
        private const decimal MAX_HOURLY_RATE = 10_000m;
        private const decimal MAX_OVERTIME_RATE = 10_000m;

        private static readonly EmployeeManager employeeManager = new();
        private static readonly TimeRecordManager timeManager = new();

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
                Console.WriteLine($"\n[FATAL] Unexpected error: {ex.Message}");
            }
            finally
            {
                // Ask to save before exit if user didn't already
                Console.WriteLine("\nExiting. Save data before leaving? (Y/n)");
                if (AskYesNo(true))
                {
                    employeeManager.SaveToFile();
                    timeManager.SaveToFile();
                }
            }
        }

        private static void AppStartup()
        {
            Console.WriteLine("Loading saved data...");
            employeeManager.LoadFromFile();
            timeManager.LoadFromFile();

            employeeManager.OnEmployeeChanged += (msg) =>
            {
                Console.WriteLine($"[NOTIFICATION] {msg}");
            };

            timeManager.OnClockEvent += (empId, time, action) =>
            {
                Console.WriteLine($"[CLOCK EVENT] {empId} - {action} at {time:HH:mm:ss}");
            };
        }

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

                int mainOpt = ReadIntInRange(0, 4, allowZero: false);
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

        #region Employee Menu
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

        private static void AddFullTimeEmployee()
        {
            Console.Write("Employee ID: ");
            string id = ReadNonEmptyTrimmed();
            if (!Validators.IsValidEmployeeIdFormat(id))
            {
                Console.WriteLine("Invalid Employee ID format. Only letters, digits, '-' and '_' are allowed.");
                return;
            }
            if (employeeManager.IdExists(id))
            {
                Console.WriteLine("Employee ID already exists.");
                return;
            }

            Console.Write("Name: ");
            string name = ReadNonEmptyTrimmed();
            if (!Validators.IsValidName(name))
            {
                Console.WriteLine("Invalid name. Must contain letters and spaces only.");
                return;
            }

            Console.Write("Email: ");
            string email = ReadValidEmail();

            if (employeeManager.EmailExists(email))
            {
                Console.WriteLine("Email already used by another employee.");
                return;
            }

            Console.Write("Hire date (yyyy-MM-dd) [leave empty = today]: ");
            DateTime hireDate = ReadOptionalDate(DateTime.Now);

            Console.Write("Monthly Salary: ");
            decimal monthly = ReadDecimalInRange(0, MAX_MONTHLY_SALARY);

            Console.Write("Overtime Rate (per hour): ");
            decimal overtime = ReadDecimalInRange(0, MAX_OVERTIME_RATE);

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

        private static void AddPartTimeEmployee()
        {
            Console.Write("Employee ID: ");
            string id = ReadNonEmptyTrimmed();
            if (!Validators.IsValidEmployeeIdFormat(id))
            {
                Console.WriteLine("Invalid Employee ID format. Only letters, digits, '-' and '_' are allowed.");
                return;
            }
            if (employeeManager.IdExists(id))
            {
                Console.WriteLine("Employee ID already exists.");
                return;
            }

            Console.Write("Name: ");
            string name = ReadNonEmptyTrimmed();
            if (!Validators.IsValidName(name))
            {
                Console.WriteLine("Invalid name. Must contain letters and spaces only.");
                return;
            }

            Console.Write("Email: ");
            string email = ReadValidEmail();
            if (employeeManager.EmailExists(email))
            {
                Console.WriteLine("Email already used by another employee.");
                return;
            }

            Console.Write("Hire date (yyyy-MM-dd) [leave empty = today]: ");
            DateTime hireDate = ReadOptionalDate(DateTime.Now);

            Console.Write("Hourly Rate: ");
            decimal rate = ReadDecimalInRange(0, MAX_HOURLY_RATE);

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

        private static void RemoveEmployee()
        {
            Console.Write("Enter Employee ID to remove: ");
            string id = ReadNonEmptyTrimmed();
            var emp = employeeManager.FindEmployeeById(id);
            if (emp == null) { Console.WriteLine("Employee not found."); return; }

            Console.WriteLine("Found:");
            emp.DisplayInfo();
            Console.WriteLine("\nAre you sure you want to delete this employee? This action cannot be undone. (y/N)");
            if (!AskYesNo(false))
            {
                Console.WriteLine("Aborted.");
                return;
            }

            if (employeeManager.RemoveEmployee(id))
                Console.WriteLine("Employee removed.");
            else
                Console.WriteLine("Removal failed.");
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
        #endregion

        #region Time Menu
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
        #endregion

        #region Reports
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

        private static void CalculatePayReport()
        {
            Console.Write("Employee ID: ");
            string id = ReadNonEmptyTrimmed();
            var emp = employeeManager.FindEmployeeById(id);
            if (emp == null) { Console.WriteLine("Employee not found."); return; }

            Console.Write("Enter start date (yyyy-MM-dd): ");
            DateTime start = ReadDateStrict();

            Console.Write("Enter end date (yyyy-MM-dd): ");
            DateTime end = ReadDateStrict();

            if (end < start) { Console.WriteLine("End date must be after start date."); return; }

            decimal totalPay = 0m;
            double totalHours = 0;

            if (emp is FullTimeEmployee ft)
            {
                totalPay = ft.CalculatePay(start, end);
            }
            else if (emp is PartTimeEmployee pt)
            {
                totalHours = timeManager.CalculateTotalHours(id, start, end);
                totalPay = pt.CalculatePay(totalHours);
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
        #endregion

        #region Helper Input Methods

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

        private static string ReadNonEmptyTrimmed()
        {
            while (true)
            {
                var s = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                Console.Write("Input required. Try again: ");
            }
        }

        private static string ReadValidEmail()
        {
            while (true)
            {
                var s = Console.ReadLine() ?? "";
                s = s.Trim();
                if (Validators.IsValidEmail(s)) return s;
                Console.Write("? Invalid email format. Try again: ");
            }
        }

        private static DateTime ReadOptionalDate(DateTime defaultValue)
        {
            var s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) return defaultValue.Date;
            if (DateTime.TryParseExact(s.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt)) return dt.Date;
            Console.WriteLine("? Invalid date. Use yyyy-MM-dd.");
            return ReadOptionalDate(defaultValue);
        }

        private static DateTime ReadDateStrict()
        {
            while (true)
            {
                var s = Console.ReadLine() ?? "";
                if (DateTime.TryParseExact(s.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dt))
                {
                    return dt.Date;
                }
                Console.Write("? Invalid date. Use yyyy-MM-dd.\nEnter date: ");
            }
        }

        private static decimal ReadDecimalInRange(decimal min, decimal max)
        {
            while (true)
            {
                var s = Console.ReadLine() ?? "";
                s = s.Trim();
                if (decimal.TryParse(s, out var d))
                {
                    if (d >= min && d <= max) return d;
                }
                Console.Write("? Invalid decimal number. Try again: ");
            }
        }

        private static bool AskYesNo(bool defaultYes)
        {
            var s = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(s)) return defaultYes;
            s = s.Trim().ToLowerInvariant();
            if (s == "y" || s == "yes") return true;
            if (s == "n" || s == "no") return false;
            return defaultYes;
        }

        #endregion
    }
}
