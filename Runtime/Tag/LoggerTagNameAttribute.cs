using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public class LoggerTagNameAttribute : Attribute
    {
        public readonly string Name;

        public LoggerTagNameAttribute(string name)
        {
            Name = name;
        }
    }
}