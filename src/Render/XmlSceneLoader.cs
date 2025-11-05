using NCalc;
using SixLabors.ImageSharp;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

namespace Limekuma.Render;

public interface IAsyncExpressionEngine
{
    Task<T?> EvalAsync<T>(string expr, dynamic scope);
    Task<dynamic?> EvalAsync(string expr, dynamic scope);
    void RegisterFunction(string name, Delegate func);
}

public sealed class NCalcExpressionEngine : IAsyncExpressionEngine
{
    private readonly ConcurrentDictionary<string, Delegate> _functions = new();

    public void RegisterFunction(string name, Delegate func) => _functions[name] = func;

    public async Task<T?> EvalAsync<T>(string expr, dynamic scope)
    {
        dynamic? result = await EvalAsync(expr, scope);
        if (result is T t)
        {
            return t;
        }

        if (result is T result1)
        {
            return result1;
        }

        if (typeof(T) != typeof(IEnumerable<dynamic>))
        {
            return (T?)Convert.ChangeType(result, typeof(T));
        }

        if (result is not IEnumerable en)
        {
            return (T?)Convert.ChangeType(result, typeof(T));
        }

        IEnumerable<dynamic> list = from dynamic item in en select item;
        return (T)list;
    }

    public async Task<dynamic?> EvalAsync(string expr, dynamic scope)
    {
        AsyncExpression expression = new(expr);
        expression.EvaluateFunctionAsync += async (name, args) =>
        {
            if (_functions.TryGetValue(name, out Delegate? func))
            {
                ParameterInfo[] parameters = func.Method.GetParameters();
                dynamic?[] funcArgs = new dynamic[args.Parameters.Length];
                for (int i = 0; i < args.Parameters.Length; ++i)
                {
                    dynamic? paramValue = await args.Parameters[i].EvaluateAsync();
                    funcArgs[i] = CoerceValue(paramValue,
                        i < parameters.Length ? parameters[i].ParameterType : typeof(object));
                }

                dynamic? result = func.DynamicInvoke(funcArgs);
                args.Result = result;
            }
        };
        expression.EvaluateParameterAsync += (name, args) =>
        {
            dynamic? value = ResolveVariable(name, scope);
            args.Result = value;
            return ValueTask.CompletedTask;
        };
        dynamic? result = await expression.EvaluateAsync();
        return result;
    }

    private dynamic? ResolveVariable(string path, dynamic scope)
    {
        dynamic? current = scope;
        foreach (string seg in path.Split('.'))
        {
            if (current is null)
            {
                return null;
            }

            if (current is not IDictionary<string, dynamic> dict)
            {
                return null;
            }

            if (!dict.TryGetValue(seg, out dynamic? v))
            {
                return null;
            }

            Type t = current.GetType();
            PropertyInfo? prop = t.GetProperty(seg);
            if (prop is not null)
            {
                current = prop.GetValue(current);
                continue;
            }

            current = v;
        }

        return current;
    }

    private dynamic? CoerceValue(dynamic? value, Type targetType)
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

public sealed class XmlSceneLoader(IAsyncExpressionEngine expr)
{
    private readonly Stack<string> _baseDirs = new();

    public async Task<Node> LoadAsync(string xmlPath, dynamic? scope)
    {
        string? baseDir = Path.GetDirectoryName(Path.GetFullPath(xmlPath));
        if (!string.IsNullOrEmpty(baseDir))
        {
            _baseDirs.Push(baseDir);
        }

        try
        {
            XDocument x = XDocument.Load(xmlPath);
            return await ParseElementAsync(x.Root!, scope);
        }
        finally
        {
            if (!string.IsNullOrEmpty(baseDir))
            {
                _baseDirs.Pop();
            }
        }
    }

    private async Task<Node> ParseElementAsync(XElement el, dynamic? scope) => el.Name.LocalName switch
    {
        "Canvas" => await ParseCanvasNodeAsync(el, scope),
        "Layer" => await ParseLayerNodeAsync(el, scope),
        "Positioned" => await ParsePositionedNodeAsync(el, scope),
        "Resized" => await ParseResizedNodeAsync(el, scope),
        "Stack" => await ParseStackNodeAsync(el, scope),
        "Image" => await ParseImageNodeAsync(el, scope),
        "Text" => await ParseTextNodeAsync(el, scope),
        "If" => await ParseIfNodeAsync(el, scope),
        "For" => await ParseForAsync(el, scope),
        "Include" => await ParseIncludeAsync(el, scope),
        _ => throw new NotSupportedException($"Unknown tag: {el.Name}")
    };

    private async Task<CanvasNode> ParseCanvasNodeAsync(XElement el, dynamic? scope) => new(
        await EvaluateAttributeAsync<int>(el, "width", scope),
        await EvaluateAttributeAsync<int>(el, "height", scope),
        await ParseChildrenAsync(el, scope),
        await TryParseColorAsync(el.Attribute("background")?.Value, scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<LayerNode> ParseLayerNodeAsync(XElement el, dynamic? scope) => new(
        await ParseChildrenAsync(el, scope),
        await EvaluateAttributeOrAsync(el, "opacity", 1f, scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<PositionedNode> ParsePositionedNodeAsync(XElement el, dynamic? scope) => new(
        new Point(await EvaluateAttributeAsync<int>(el, "x", scope), await EvaluateAttributeAsync<int>(el, "y", scope)),
        (await ParseChildrenAsync(el, scope)).Single(),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<ResizedNode> ParseResizedNodeAsync(XElement el, dynamic? scope) => new(
        (await ParseChildrenAsync(el, scope)).Single(),
        await ParseSizeAsync(el, scope),
        await EvaluateAttributeOrAsync(el, "scale", 1f, scope),
        await ParseResamplerTypeAsync(el, scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<StackNode> ParseStackNodeAsync(XElement el, dynamic? scope) => new(
        await GetAttributeValueAsync(el, "direction", scope) == "row" ? StackDirection.Row : StackDirection.Column,
        await EvaluateAttributeOrAsync(el, "spacing", 0, scope),
        await ParseChildrenAsync(el, scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<ImageNode> ParseImageNodeAsync(XElement el, dynamic? scope) => new(
        await GetAttributeValueAsync(el, "namespace", scope),
        await GetAttributeValueAsync(el, "id", scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<TextNode> ParseTextNodeAsync(XElement el, dynamic? scope) => new(
        await GetAttributeValueAsync(el, "value", scope),
        await GetAttributeValueAsync(el, "fontFamily", scope),
        await EvaluateAttributeAsync<float>(el, "fontSize", scope),
        Color.Parse(await GetAttributeValueAsync(el, "color", scope)),
        await TryParseColorAsync(el.Attribute("strokeColor")?.Value, scope),
        await EvaluateAttributeOrAsync<float?>(el, "strokeWidth", null, scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<LayerNode> ParseIfNodeAsync(XElement el, dynamic? scope)
    {
        if (await EvaluateAttributeAsync<bool>(el, "test", scope))
        {
            return new(await ParseChildrenAsync(el, scope));
        }

        return new([]);
    }

    private async Task<List<Node>> ParseChildrenAsync(XElement el, dynamic? scope)
    {
        List<Node> children = [];
        foreach (XElement child in el.Elements())
        {
            children.Add(await ParseElementAsync(child, scope));
        }

        return children;
    }

    private async Task<LayerNode> ParseForAsync(XElement el, dynamic? scope)
    {
        string itemsExpr = await GetAttributeValueAsync(el, "items", scope);
        IEnumerable<dynamic>? items = await expr.EvalAsync<IEnumerable<dynamic>>(itemsExpr, scope);
        if (items is null)
        {
            return new([]);
        }

        string varName = el.Attribute("var")?.Value ?? "item";
        string idxName = el.Attribute("indexVar")?.Value ?? "idx";
        List<Node> list = [];
        int i = 0;
        foreach (dynamic item in items)
        {
            Dictionary<string, dynamic?> childScope = new() { [varName] = item, [idxName] = i, ["$parent"] = scope };
            foreach (XElement child in el.Elements())
            {
                list.Add(await ParseElementAsync(child, childScope));
            }

            ++i;
        }

        return new(list);
    }

    private async Task<Node> ParseIncludeAsync(XElement el, dynamic? scope)
    {
        if (el.Attribute("src") is not { } s)
        {
            throw new NotSupportedException("Include requires src attribute");
        }

        string rel = await GetRawAttributeValueAsync(s.Value, scope);
        string path = Path.IsPathRooted(rel) ? rel : Path.Combine(_baseDirs.Peek(), rel);
        string includeBase = Path.GetDirectoryName(Path.GetFullPath(path))!;
        _baseDirs.Push(includeBase);
        try
        {
            XDocument x = XDocument.Load(path);
            return await ParseElementAsync(x.Root!, scope);
        }
        finally
        {
            _baseDirs.Pop();
        }
    }

    private async Task<string> GetAttributeValueAsync(XElement el, string name, dynamic? scope) =>
        await EvaluateStringAsync(el.Attribute(name)?.Value, scope);

    private async Task<string> GetAttributeValueOrAsync(XElement el, string name, string defaultValue, dynamic? scope)
    {
        if (el.Attribute(name) is null)
        {
            return defaultValue;
        }

        return await EvaluateStringAsync(el.Attribute(name)?.Value, scope);
    }

    private async Task<string> GetRawAttributeValueAsync(string? raw, dynamic? scope) =>
        await EvaluateStringAsync(raw, scope);

    private async Task<string> EvaluateStringAsync(string? s, dynamic? scope)
    {
        if (s is null)
        {
            return "[NIL]";
        }

        int start = 0;
        while (true)
        {
            int i = s.IndexOf('{', start);
            if (i < 0)
            {
                break;
            }

            int j = s.IndexOf('}', i + 1);
            if (j < 0)
            {
                break;
            }

            string expr1 = s.Substring(i + 1, j - i - 1);
            dynamic val = await expr.EvalAsync(expr1, scope) ?? "[NIL]";
            s = s[..i] + val + s[(j + 1)..];
            start = i + (val?.ToString()?.Length ?? 0);
        }

        return s;
    }

    private async Task<T> EvaluateAttributeAsync<T>(XElement el, string name, dynamic? scope)
    {
        string value = await EvaluateStringAsync(el.Attribute(name)?.Value, scope);
        return (T)Convert.ChangeType(value, typeof(T));
    }

    private async Task<T> EvaluateAttributeOrAsync<T>(XElement el, string name, T defaultValue, dynamic? scope)
    {
        if (el.Attribute(name) is not { } attr)
        {
            return defaultValue;
        }

        string value = await EvaluateStringAsync(attr.Value, scope);
        if (defaultValue is null && string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }

    private async Task<Size?> ParseSizeAsync(XElement el, dynamic? scope)
    {
        string? width = el.Attribute("width")?.Value;
        string? height = el.Attribute("height")?.Value;
        if (width is null)
        {
            return null;
        }

        if (height is null)
        {
            return null;
        }

        return new(Convert.ToInt32(await EvaluateStringAsync(width, scope)),
            Convert.ToInt32(await EvaluateStringAsync(height, scope)));
    }

    private async Task<Color?> TryParseColorAsync(string? raw, dynamic? scope)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        string parsed = await EvaluateStringAsync(raw, scope);
        return Color.Parse(parsed);
    }

    private async Task<ResamplerType> ParseResamplerTypeAsync(XElement el, dynamic? scope)
    {
        string resamplerName = await GetAttributeValueOrAsync(el, "resampler", "Lanczos3", scope);
        if (Enum.TryParse(resamplerName, true, out ResamplerType resampler))
        {
            return resampler;
        }

        return ResamplerType.Lanczos3;
    }
}