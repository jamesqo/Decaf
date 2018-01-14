using System;
using System.Collections.Generic;

namespace CoffeeMachine.Internal
{
    internal static class NetCoreShims
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        public static string[] Split(this string value, char separator, StringSplitOptions options = StringSplitOptions.None)
        {
            return value.Split(new[] { separator }, options);
        }

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                return false;
            }

            dictionary.Add(key, value);
            return true;
        }
    }
}
