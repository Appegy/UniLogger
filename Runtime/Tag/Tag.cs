using System;

namespace UnityEngine
{
    public readonly struct Tag : IEquatable<Tag>
    {
        public readonly string Category;
        public readonly string Name;

        public Tag(string category, string name)
        {
            Category = category;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        internal string ToLongString()
        {
            return Category + "." + Name;
        }

        public override bool Equals(object other)
        {
            return other is Tag category && Equals(category);
        }

        public bool Equals(Tag other)
        {
            return (Tag: Category, Type: Name).Equals((other.Category, other.Name));
        }

        public override int GetHashCode()
        {
            return (Tag: Category, Type: Name).GetHashCode();
        }
    }
}