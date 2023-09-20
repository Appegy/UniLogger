using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine
{
    public enum LogLevel
    {
        Trace = 0,
        Log = 1,
        Warning = 2,
        Error = 3,
        Exception = 4,
    }

    public static class LogLevelExtensions
    {
        public static readonly IReadOnlyList<LogLevel> LogTypes = Enum.GetValues(typeof(LogType)).Cast<LogLevel>().ToArray();

        public static LogLevel ConvertToLogLevel(this LogType original)
        {
            return original switch
            {
                LogType.Error => LogLevel.Error,
                LogType.Assert => LogLevel.Error,
                LogType.Warning => LogLevel.Warning,
                LogType.Log => LogLevel.Log,
                LogType.Exception => LogLevel.Exception,
                _ => throw new ArgumentOutOfRangeException(nameof(original), original, null)
            };
        }

        public static LogType ConvertToLogType(this LogLevel original)
        {
            return original switch
            {
                LogLevel.Trace => LogType.Log,
                LogLevel.Log => LogType.Log,
                LogLevel.Warning => LogType.Warning,
                LogLevel.Error => LogType.Error,
                LogLevel.Exception => LogType.Exception,
                _ => throw new ArgumentOutOfRangeException(nameof(original), original, null)
            };
        }

        public static string ToShortString(this LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TR",
                LogLevel.Log => "LG",
                LogLevel.Warning => "WN",
                LogLevel.Error => "ER",
                LogLevel.Exception => "EX",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }

        public static string ToMessageColor(this LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "#4DA6FF", // light blue
                LogLevel.Log => "white",
                LogLevel.Warning => "orange",
                LogLevel.Error => "red",
                LogLevel.Exception => "#FF5349FF", // (orange red)
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }
    }
}