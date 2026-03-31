using NCalc;
using System.Collections;
using System.Globalization;

namespace Limekuma.Render.ExpressionEngine;

public sealed class AsyncNCalcEngine
{
    public async Task<T?> EvalAsync<T>(string expr, object? scope)
    {
        object? result = await EvalAsync(expr, scope);
        if (result is T t)
        {
            return t;
        }

        if (result is null)
        {
            return default;
        }

        if (typeof(T) == typeof(IEnumerable<object>) && result is IEnumerable enumerable)
        {
            List<object> list = [];
            foreach (object? item in enumerable)
            {
                if (item is not null)
                {
                    list.Add(item);
                }
            }

            return (T)(object)list;
        }

        return (T?)ConvertValue(result, typeof(T));
    }

    public async Task<object?> EvalAsync(string expr, object? scope)
    {
        AsyncExpression expression = new(expr);
        Dictionary<string, object?> flattened = ScopeFlattener.Flatten(scope);
        foreach ((string key, object? value) in flattened)
        {
            expression.Parameters[key] = value is Enum e ? Convert.ToInt32(e, CultureInfo.InvariantCulture) : value;
        }

        return await expression.EvaluateAsync();
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value is null)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        Type finalTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (finalTarget.IsInstanceOfType(value))
        {
            return value;
        }

        if (finalTarget.IsEnum)
        {
            if (value is string enumText)
            {
                return Enum.Parse(finalTarget, enumText, true);
            }

            object enumValue = Convert.ChangeType(value, Enum.GetUnderlyingType(finalTarget), CultureInfo.InvariantCulture);
            return Enum.ToObject(finalTarget, enumValue);
        }

        if (finalTarget == typeof(bool))
        {
            if (value is string sv)
            {
                if (bool.TryParse(sv, out bool b))
                {
                    return b;
                }

                if (double.TryParse(sv, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                {
                    return Math.Abs(d) > 0.0000001;
                }
            }

            if (value is IConvertible convertible)
            {
                return Math.Abs(convertible.ToDouble(CultureInfo.InvariantCulture)) > 0.0000001;
            }
        }

        if (value is IConvertible)
        {
            return Convert.ChangeType(value, finalTarget, CultureInfo.InvariantCulture);
        }

        return finalTarget == typeof(string) ? value.ToString() : value;
    }
}