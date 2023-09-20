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
        TagCategory = 8,
        TagName = 16,
        LogType = 32,
    }
}