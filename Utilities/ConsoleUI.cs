using System;

namespace EmployeeTimeTracker.Utilities
{
    public static class ConsoleUI
    {
        public static void Print(string message)
        {
            Console.Write(message);
        }

        public static void Println(string message = "")
        {
            Console.WriteLine(message);
        }

        public static void Header(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== " + title + " ===");
            Console.ResetColor();
        }

        public static void Success(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
