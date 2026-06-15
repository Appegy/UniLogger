using System;

namespace UnityEngine
{
    [Flags]
    public enum FormatOptions
    {
        None = 0,
        RichText = 1,
        Time = 2,
        Thread = 4,
        Tags = 8,
        LogType = 16,
    }

    public static class FormatOptionsExtensions
    {
        public static bool HasFlagFast(this FormatOptions value, FormatOptions flag)
        {
            return (value & flag) != 0;
        }
    }
}