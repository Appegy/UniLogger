using System;
using System.Collections.Concurrent;

namespace Appegy.UniLogger
{
    internal static class EnumTagCache<TEnum>
        where TEnum : struct, Enum
    {
        private static readonly ConcurrentDictionary<TEnum, string> _cache = new();
        private static readonly Func<TEnum, string> _resolve = Resolve;

        public static string Get(TEnum value)
        {
            return _cache.GetOrAdd(value, _resolve);
        }

        private static string Resolve(TEnum value)
        {
            var attributes = (TagNameAttribute[])typeof(TEnum).GetField(value.ToString()).GetCustomAttributes(typeof(TagNameAttribute), false);
            return attributes.Length > 0 ? attributes[0].Name : value.ToString();
        }
    }
}
