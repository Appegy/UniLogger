using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class LoggerTagsContainerAttribute : Attribute
    {
        public readonly string Category;

        public LoggerTagsContainerAttribute(string category)
        {
            Category = category;
        }
    }
}