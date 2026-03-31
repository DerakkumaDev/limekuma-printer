using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Limekuma.Render.ExpressionEngine;

internal static class ScopeFlattener
{
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
        if (type.IsPrimitive || value is string || value is decimal || value is DateTime || value is DateTimeOffset ||
            value is TimeSpan || value is Guid || value is Enum)
        {
            return;
        }

        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is null)
                {
                    continue;
                }

                string key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? string.Empty;
                string childPrefix = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
                FlattenObject(output, childPrefix, entry.Value, depth + 1);
            }
            return;
        }

        if (value is IEnumerable && value is not string)
        {
            return;
        }

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (PropertyInfo property in properties)
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            object? propertyValue = property.GetValue(value);
            string childName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
            FlattenObject(output, childName, propertyValue, depth + 1);
        }
    }
}