using System;

namespace Lightning.Metrics
{
    public class Logger
    {
        public static void Debug(string message)
        {
            Console.WriteLine($"{DateTime.Now:o} DEBUG {message}");
        }

        public static void Error(string message)
        {
            Console.WriteLine($"{DateTime.Now:o} ERROR {message}");
        }
    }
}