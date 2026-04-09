using NCalc;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Limekuma.Render.ExpressionEngine;

public sealed class AsyncNCalcEngine
{
    private readonly ConcurrentDictionary<string, Delegate> _functions = new();

    public void RegisterFunction(string name, Delegate func) => _functions[name] = func;

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

        if (typeof(T) != typeof(IEnumerable<object>))
        {
            return (T?)ConvertValue(result, typeof(T));
        }

        if (result is not IEnumerable enumerable)
        {
            return (T?)ConvertValue(result, typeof(T));
        }

        List<object> list = [];
        list.AddRange(enumerable.OfType<object>());

        return (T)(object)list;
    }

    public async Task<object?> EvalAsync(string expr, object? scope)
    {
        AsyncExpression expression = new(expr);
        Dictionary<string, object?> flattened = ScopeFlattener.Flatten(scope);
        foreach ((string key, object? value) in flattened)
        {
            expression.Parameters[key] = value is Enum e ? Convert.ToInt32(e, CultureInfo.InvariantCulture) : value;
        }

        expression.EvaluateFunctionAsync += async (name, args) =>
        {
            if (!_functions.TryGetValue(name, out Delegate? func))
            {
                return;
            }

            ParameterInfo[] parameters = func.Method.GetParameters();
            object?[] funcArgs = new object[args.Parameters.Length];
            for (int i = 0; i < args.Parameters.Length; ++i)
            {
                object? paramValue = await args.Parameters[i].EvaluateAsync();
                funcArgs[i] = CoerceValue(paramValue,
                    i < parameters.Length ? parameters[i].ParameterType : typeof(object));
            }

            object? result = func.DynamicInvoke(funcArgs);
            args.Result = result;
        };

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

            object enumValue =
                Convert.ChangeType(value, Enum.GetUnderlyingType(finalTarget), CultureInfo.InvariantCulture);
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

    private object? CoerceValue(object? value, Type targetType)
    {
        if (value is null)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : value;
        }

        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        if (targetType == typeof(string))
        {
            return value.ToString();
        }

        if (targetType == typeof(bool))
        {
            if (value is string sv)
            {
                if (bool.TryParse(sv, out bool bv))
                {
                    return bv;
                }

                if (double.TryParse(sv, out double dv))
                {
                    return Math.Abs(dv) > 0.0000001;
                }
            }

            if (value is not IConvertible conv)
            {
                return false;
            }

            double dvd = conv.ToDouble(CultureInfo.InvariantCulture);
            return Math.Abs(dvd) > 0.0000001;
        }

        if (!typeof(IConvertible).IsAssignableFrom(targetType))
        {
            return value;
        }

        if (value is IConvertible)
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        if (value is string s2)
        {
            return Convert.ChangeType(s2, targetType, CultureInfo.InvariantCulture);
        }

        return value;
    }
}
