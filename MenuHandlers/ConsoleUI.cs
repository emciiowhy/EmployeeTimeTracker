using System;

namespace EmployeeTimeTracker.MenuHandlers
{
    public static class ConsoleUI
    {
        public static void Section(string title)
        {
            Console.WriteLine($"\n--- {title} ---");
        }

        public static void Error(string message)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {message}");
            Console.ForegroundColor = prev;
        }
    }
}
