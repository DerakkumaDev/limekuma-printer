using Limekuma.Render.ExpressionEngine;
using Limekuma.Render.Nodes;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System.Xml.Linq;

namespace Limekuma.Render;

public sealed class XmlSceneLoader(IAsyncExpressionEngine expr)
{
    private readonly Stack<string> _baseDirs = new();

    public async Task<Node> LoadAsync(string xmlPath, object? scope)
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

    private async Task<Node> ParseElementAsync(XElement el, object? scope) => el.Name.LocalName switch
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

    private async Task<CanvasNode> ParseCanvasNodeAsync(XElement el, object? scope) => new(
        await EvaluateAttributeAsync<int>(el, "width", scope),
        await EvaluateAttributeAsync<int>(el, "height", scope),
        await TryParseColorAsync(el.Attribute("background")?.Value, scope),
        await ParseChildrenAsync(el, scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<LayerNode> ParseLayerNodeAsync(XElement el, object? scope) => new(
        await EvaluateAttributeOrAsync<float>(el, "opacity", 1, scope),
        await ParseChildrenAsync(el, scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<PositionedNode> ParsePositionedNodeAsync(XElement el, object? scope) => new(
        new Point(await EvaluateAttributeAsync<int>(el, "x", scope), await EvaluateAttributeAsync<int>(el, "y", scope)),
        await ParseChildrenAsync(el, scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<ResizedNode> ParseResizedNodeAsync(XElement el, object? scope) => new(
        await EvaluateAttributeOrAsync<float>(el, "scale", 1, scope),
        await ParseSizeAsync(el, scope),
        await ParseResamplerTypeAsync(el, scope),
        (await ParseChildrenAsync(el, scope)).Single(),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<StackNode> ParseStackNodeAsync(XElement el, object? scope) => new(
        await GetAttributeValueAsync(el, "direction", scope) is "row" ? StackDirection.Row : StackDirection.Column,
        await EvaluateAttributeOrAsync(el, "spacing", 0, scope),
        await ParseChildrenAsync(el, scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<ImageNode> ParseImageNodeAsync(XElement el, object? scope) => new(
        await GetAttributeValueAsync(el, "namespace", scope),
        await GetAttributeValueAsync(el, "id", scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<TextNode> ParseTextNodeAsync(XElement el, object? scope) => new(
        await GetAttributeValueAsync(el, "value", scope),
        await GetAttributeValueAsync(el, "fontFamily", scope),
        await EvaluateAttributeAsync<int>(el, "fontSize", scope),
        Color.Parse(await GetAttributeValueAsync(el, "color", scope)),
        await EvaluateAttributeOrAsync(el, "align", TextAlignment.Start, scope),
        await EvaluateAttributeOrAsync(el, "alignV", VerticalAlignment.Top, scope),
        await EvaluateAttributeOrAsync(el, "alignH", HorizontalAlignment.Left, scope),
        await TryParseColorAsync(el.Attribute("strokeColor")?.Value, scope),
        await EvaluateAttributeOrAsync<float>(el, "strokeWidth", 0, scope),
        await EvaluateAttributeOrAsync<float?>(el, "truncateWidth", null, scope),
        await EvaluateAttributeOrAsync(el, "truncateSubfix", "", scope),
        await GetAttributeValueAsync(el, "key", scope)
    );

    private async Task<LayerNode> ParseIfNodeAsync(XElement el, object? scope)
    {
        if (await EvaluateAttributeAsync<bool>(el, "test", scope))
        {
            return new(1, await ParseChildrenAsync(el, scope), null);
        }

        return new(1, [], null);
    }

    private async Task<List<Node>> ParseChildrenAsync(XElement el, object? scope)
    {
        List<Node> children = [];
        foreach (XElement child in el.Elements())
        {
            children.Add(await ParseElementAsync(child, scope));
        }

        return children;
    }

    private async Task<LayerNode> ParseForAsync(XElement el, object? scope)
    {
        string itemsExpr = await GetAttributeValueAsync(el, "items", scope);
        IEnumerable<object>? items = await expr.EvalAsync<IEnumerable<object>>(itemsExpr, scope);
        if (items is null)
        {
            return new(1, [], null);
        }

        string varName = el.Attribute("var")?.Value ?? "item";
        string idxName = el.Attribute("indexVar")?.Value ?? "idx";
        List<Node> list = [];
        int i = 0;
        foreach (object item in items)
        {
            Dictionary<string, object?> childScope = new()
            {
                [varName] = item,
                [idxName] = i,
                ["$parent"] = scope
            };
            foreach (XElement child in el.Elements())
            {
                list.Add(await ParseElementAsync(child, childScope));
            }

            ++i;
        }

        return new(1, list, null);
    }

    private async Task<Node> ParseIncludeAsync(XElement el, object? scope)
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

    private async Task<string> GetAttributeValueAsync(XElement el, string name, object? scope) =>
        await EvaluateStringAsync(el.Attribute(name)?.Value, scope);

    private async Task<string> GetAttributeValueOrAsync(XElement el, string name, string defaultValue, object? scope)
    {
        if (el.Attribute(name) is null)
        {
            return defaultValue;
        }

        return await EvaluateStringAsync(el.Attribute(name)?.Value, scope);
    }

    private async Task<string> GetRawAttributeValueAsync(string? raw, object? scope) =>
        await EvaluateStringAsync(raw, scope);

    private async Task<string> EvaluateStringAsync(string? s, object? scope)
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
            object val = await expr.EvalAsync(expr1, scope) ?? "[NIL]";
            s = s[..i] + val + s[(j + 1)..];
            start = i + (val?.ToString()?.Length ?? 0);
        }

        return s;
    }

    private async Task<T> EvaluateAttributeAsync<T>(XElement el, string name, object? scope)
    {
        string value = await EvaluateStringAsync(el.Attribute(name)?.Value, scope);
        return (T)Convert.ChangeType(value, typeof(T));
    }

    private async Task<T> EvaluateAttributeOrAsync<T>(XElement el, string name, T defaultValue, object? scope)
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

    private async Task<Size?> ParseSizeAsync(XElement el, object? scope)
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

    private async Task<Color?> TryParseColorAsync(string? raw, object? scope)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        string parsed = await EvaluateStringAsync(raw, scope);
        return Color.Parse(parsed);
    }

    private async Task<ResamplerType> ParseResamplerTypeAsync(XElement el, object? scope)
    {
        string resamplerName = await GetAttributeValueOrAsync(el, "resampler", "Lanczos3", scope);
        if (Enum.TryParse(resamplerName, true, out ResamplerType resampler))
        {
            return resampler;
        }

        return ResamplerType.Lanczos3;
    }
}