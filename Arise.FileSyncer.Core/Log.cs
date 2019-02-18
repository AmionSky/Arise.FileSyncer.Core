using System;

namespace Arise.FileSyncer
{
    public static class Log
    {
        public delegate void DelegateLog(string message);

        public static DelegateLog Error = DefaultError;
        public static DelegateLog Warning = DefaultWarning;
        public static DelegateLog Info = DefaultInfo;
        public static DelegateLog Verbose = DefaultVerbose;
        public static DelegateLog Debug = DefaultDebug;

        private static void DefaultError(string message)
        {
            Console.WriteLine(Format("E: " + message));
        }

        private static void DefaultWarning(string message)
        {
            Console.WriteLine(Format("W: " + message));
        }

        private static void DefaultInfo(string message)
        {
            Console.WriteLine(Format("I: " + message));
        }

        private static void DefaultVerbose(string message)
        {
            Console.WriteLine(Format("V: " + message));
        }

        private static void DefaultDebug(string message)
        {
            Console.WriteLine(Format("D: " + message));
        }

        private static string Format(string message)
        {
            var now = DateTime.Now;
            return $"{now.Year.ToString("0000")}-{now.Month.ToString("00")}-{now.Day.ToString("00")} " +
            $"{now.Hour.ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")} | {message}";
        }
    }
}
