using System;

namespace SteamAccountCreator.Utils
{
    public static class Log
    {
        public static void Info(string message)
        {
            WriteLine(message, "INFO");
        }

        public static void Warn(string message)
        {
            WriteLine(message, "WARN");
        }

        public static void Error(string message)
        {
            WriteLine(message, "ERROR");
        }
        
        static void WriteLine(string message, string level)
        {
            string timeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            Console.WriteLine($"[{timeStamp}] {level}: {message}");
        }
    }
}