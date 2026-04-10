using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;

namespace Limekuma.Render.ExpressionEngine;

internal static class ScopeFlattener
{
    private static readonly ConcurrentDictionary<Type, ImmutableArray<PropertyInfo>> PropertyCache = new();
    private static readonly ConcurrentDictionary<Type, bool> LeafTypeCache = new();

    public static Dictionary<string, object?> Flatten(object? scope)
    {
        Dictionary<string, object?> result = new(StringComparer.OrdinalIgnoreCase);
        if (scope is null)
        {
            return result;
        }

        FlattenObject(result, string.Empty, scope, 0);
        return result;
    }

    private static void FlattenObject(IDictionary<string, object?> output, string prefix, object? value, int depth)
    {
        if (depth > 6)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                output[prefix] = value;
            }

            return;
        }

        if (!string.IsNullOrEmpty(prefix))
        {
            output[prefix] = value;
        }

        if (value is null)
        {
            return;
        }

        Type type = value.GetType();
        if (IsLeafType(type))
        {
            return;
        }

        if (value is IDictionary<string, object?> genericDictionary)
        {
            foreach ((string key, object? entryValue) in genericDictionary)
            {
                string childPrefix = prefix.Length is 0 ? key : string.Concat(prefix, ".", key);
                FlattenObject(output, childPrefix, entryValue, depth + 1);
            }

            return;
        }

        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                string key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? string.Empty;
                string childPrefix = prefix.Length is 0 ? key : string.Concat(prefix, ".", key);
                FlattenObject(output, childPrefix, entry.Value, depth + 1);
            }

            return;
        }

        if (value is IEnumerable and not string)
        {
            return;
        }

        ImmutableArray<PropertyInfo> properties = PropertyCache.GetOrAdd(type,
            static t =>
            [
                .. t.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(property =>
                    property.CanRead && property.GetIndexParameters().Length is 0)
            ]);
        foreach (PropertyInfo property in properties)
        {
            object? propertyValue = property.GetValue(value);
            string childName = prefix.Length is 0 ? property.Name : string.Concat(prefix, ".", property.Name);
            FlattenObject(output, childName, propertyValue, depth + 1);
        }
    }

    private static bool IsLeafType(Type type) => LeafTypeCache.GetOrAdd(type,
        static t => t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime) ||
                    t == typeof(DateTimeOffset) || t == typeof(TimeSpan) || t == typeof(Guid));
}
