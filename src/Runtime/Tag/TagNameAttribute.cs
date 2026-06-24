using System;

namespace Appegy.UniLogger
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
    public class TagNameAttribute : Attribute
    {
        public readonly string Name;

        public TagNameAttribute(string name)
        {
            Name = name;
        }
    }
}