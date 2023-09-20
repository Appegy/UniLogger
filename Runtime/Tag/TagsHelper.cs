using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace UnityEngine
{
    internal static class TagsHelper
    {
        private static readonly Dictionary<Type, string> _tagCategoryCache = new Dictionary<Type, string>();
        private static readonly Dictionary<Enum, string> _tagNamesCache = new Dictionary<Enum, string>();

        public static Tag GetTag<TLoggerTag>(this TLoggerTag tag)
            where TLoggerTag : struct, Enum
        {
            return new Tag(GetTagCategory(typeof(TLoggerTag)), GetTagName(tag));
        }

        private static string GetTagCategory(Type value)
        {
            if (!_tagCategoryCache.TryGetValue(value, out var name))
            {
                var attributes = (LoggerTagsContainerAttribute[])value.GetCustomAttributes(typeof(LoggerTagsContainerAttribute), false);
                name = attributes.Length > 0 ? attributes[0].Category : value.ToString();
                _tagCategoryCache[value] = name;
            }
            return name;
        }

        private static string GetTagName(Enum value)
        {
            if (!_tagNamesCache.TryGetValue(value, out var name))
            {
                var attributes = (EnumMemberAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(EnumMemberAttribute), false);
                name = attributes.Length > 0 ? attributes[0].Value : value.ToString();
                _tagNamesCache[value] = name;
            }
            return name;
        }
    }
}