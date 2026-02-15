using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static UiTools.WinForms.Designer.Core.MessageLogger;

namespace UiTools.WinForms.Designer.Core
{
    public static class MessageLogger
    {
        private static ILogTarget logTarget;
        private static LogLevel minLogLevel;
        private static readonly object objLock = new object();

        public enum LogLevel : int
        {
            Verbose = 0,
            Info,
            Warning,
            Error
        }

        public static void Init(ILogTarget target, LogLevel logLevel)
        {
            lock (objLock)
            {
                logTarget = target;
                minLogLevel = logLevel;
            }
        }

        public static void Log(object caller, string msg, LogLevel level = LogLevel.Info, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(level, caller.GetType(), msg, null, memberName);
        }
        public static void Log(Type callerType, string msg, LogLevel level = LogLevel.Info, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(level, callerType, msg, null, memberName);
        }

        public static void LogVerbose(object caller, string msg, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(LogLevel.Verbose, caller.GetType(), msg, null, memberName);
        }
        public static void LogVerbose(Type callerType, string msg, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(LogLevel.Verbose, callerType, msg, null, memberName);
        }

        public static void LogWarning(object caller, string msg, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(LogLevel.Warning, caller.GetType(), msg, null, memberName);
        }
        public static void LogWarning(Type callerType, string msg, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(LogLevel.Warning, callerType, msg, null, memberName);
        }

        public static void LogError(object caller, string msg, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(LogLevel.Error, caller.GetType(), msg, null, memberName);
        }
        public static void LogError(Type callerType, string msg, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(LogLevel.Error, callerType, msg, null, memberName);
        }

        public static void LogError(object caller, string msg, Exception ex, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(LogLevel.Error, caller.GetType(), msg, ex, memberName);
        }
        public static void LogError(Type callerType, string msg, Exception ex, [CallerMemberName] string memberName = "")
        {
            LogMessageInternal(LogLevel.Error, callerType, msg, ex, memberName);
        }

        private static void LogMessageInternal(LogLevel level, Type callerType, string msg, Exception ex, string memberName)
        {
            if (logTarget == null)
                return;

            if ((int)level < (int)minLogLevel)
                return;

            ILogTarget currentTarget;
            lock (objLock)
                currentTarget = logTarget;

            var prefix = level == LogLevel.Info ? "" : $"[{level}] ";
            if (currentTarget == null)
                Debug.WriteLine($"{prefix}{callerType.Name}.{memberName}: {msg}"); // fallback
            else
                currentTarget.WriteLine(level, $"{callerType.Name}.{memberName}: {msg}", ex);
        }
    }

    public interface ILogTarget
    {
        void WriteLine(LogLevel level, string message, Exception ex);
    }
}
