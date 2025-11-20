using System;
using System.Linq;
using System.Collections.Generic;
using EmployeeTimeTracker.Managers;
using EmployeeTimeTracker.Models;
using EmployeeTimeTracker.Utilities;

namespace EmployeeTimeTracker.MenuHandlers
{
    public class EmployeeMenu
    {
        private readonly EmployeeManager _employeeManager;

        public EmployeeMenu(EmployeeManager employeeManager)
        {
            _employeeManager = employeeManager;
        }

        public void ShowEmployeeSearchMenu()
        {
            ConsoleUI.Section("Employee Search");
            Console.Write("Enter employee name (full or partial): ");
            string input = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                ConsoleUI.Error("Invalid input.");
                return;
            }

            var matches = _employeeManager.SearchEmployeesByName(input);

            if (matches.Count == 0)
            {
                Console.WriteLine("No employees found.");
                return;
            }

            Console.WriteLine("\nSearch Results:");
            int index = 1;
            foreach (var emp in matches)
            {
                Console.WriteLine($"[{index}] {emp.EmployeeId} | {emp.Name} - {emp.GetType().Name}");
                index++;
            }

            if (matches.Count == 1)
            {
                Console.WriteLine("\nPress any key to view details...");
                Console.ReadKey();
                Console.WriteLine("\n--- EMPLOYEE DETAILS ---");
                matches[0].DisplayInfo();
                return;
            }

            Console.Write("\nEnter selection number to view details (0 to cancel): ");
            string? sel = Console.ReadLine();
            if (!int.TryParse(sel, out int selIdx) || selIdx < 0 || selIdx > matches.Count)
            {
                ConsoleUI.Error("Invalid selection.");
                return;
            }
            if (selIdx == 0) return;

            var selected = matches[selIdx - 1];
            Console.WriteLine("\n--- EMPLOYEE DETAILS ---");
            selected.DisplayInfo();
        }
    }
}
