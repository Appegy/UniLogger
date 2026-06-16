using System;
using System.Collections.Generic;

namespace UnityEngine
{
    internal static class TagsHelper
    {
        private static readonly Dictionary<Type, string> _tagTypesCache = new Dictionary<Type, string>();
        private static readonly Dictionary<Enum, string> _tagEnumsCache = new Dictionary<Enum, string>();

        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static string GetTag(this object tag)
        {
            return tag switch
            {
                null => "NULL",
                string stringTag => stringTag,
                Type typeTag => typeTag.GetTag(),
                Enum enumTag => enumTag.GetTag(),
                _ => tag.ToString()
            };
        }

        public static string GetTag(this Enum value)
        {
            if (!_tagEnumsCache.TryGetValue(value, out var name))
            {
                var attributes = (TagNameAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(TagNameAttribute), false);
                name = attributes.Length > 0 ? attributes[0].Name : value.ToString();
                _tagEnumsCache[value] = name;
            }
            return name;
        }

        public static string GetTag(this Type value)
        {
            if (!_tagTypesCache.TryGetValue(value, out var name))
            {
                var attributes = (TagNameAttribute[])value.GetCustomAttributes(typeof(TagNameAttribute), false);
                name = attributes.Length > 0 ? attributes[0].Name : value.Name;
                _tagTypesCache[value] = name;
            }
            return name;
        }
    }
}