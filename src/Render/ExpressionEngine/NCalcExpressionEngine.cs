using NCalc;
using SixLabors.ImageSharp;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Limekuma.Render.ExpressionEngine;

public sealed class NCalcExpressionEngine : IAsyncExpressionEngine
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

        if (result is T result1)
        {
            return result1;
        }

        if (typeof(T) != typeof(IEnumerable<object>))
        {
            return (T?)Convert.ChangeType(result, typeof(T));
        }

        if (result is not IEnumerable en)
        {
            return (T?)Convert.ChangeType(result, typeof(T));
        }

        IEnumerable<object> list = from object item in en select item;
        return (T)list;
    }

    public async Task<object?> EvalAsync(string expr, object? scope)
    {
        Console.WriteLine(expr);
        AsyncExpression expression = new(expr);
        expression.EvaluateFunctionAsync += async (name, args) =>
        {
            Console.WriteLine("fun " + name);
            if (_functions.TryGetValue(name, out Delegate? func))
            {
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
            }
        };
        expression.EvaluateParameterAsync += (name, args) =>
        {
            object? value = ResolveVariable(name, scope);
            if (value is Enum e)
            {
                value = Convert.ToInt32(e);
            }

            args.Result = value;
            Console.WriteLine("var " + name + " " + value);
            return ValueTask.CompletedTask;
        };
        object? result = await expression.EvaluateAsync();
        return result;
    }

    private object? ResolveVariable(string path, object? scope)
    {
        object? current = scope;
        foreach (string seg in path.Split('_'))
        {
            Console.WriteLine("seg " + seg);
            if (current is null)
            {
                return current;
            }

            if (current is IDictionary<string, object> dict)
            {
                if (dict.TryGetValue(seg, out current))
                {
                    continue;
                }

                return dict;
            }

            Type t = current.GetType();
            PropertyInfo? prop = t.GetProperty(seg);
            if (prop is null)
            {
                return current;
            }

            current = prop.GetValue(current);
        }

        return current;
    }

    private object? CoerceValue(object? value, Type targetType)
    {
        if (value is null)
        {
            if (targetType.IsValueType)
            {
                return Activator.CreateInstance(targetType);
            }

            return value;
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
