using Limekuma.Render.Nodes;
using Limekuma.Render.Types;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections;
using System.Globalization;
using System.Xml.Linq;

namespace Limekuma.Render;

public sealed partial class TemplateReader
{
    private async Task<Node> ParseCanvasNodeAsync(XElement element, object? scope) =>
        new CanvasNode(
            await EvaluateRequiredExpressionAttributeAsync<int>(element, "width", scope),
            await EvaluateRequiredExpressionAttributeAsync<int>(element, "height", scope),
            await ParseColorAttributeAsync(element, "background", scope),
            await ParseChildrenAsync(element, scope),
            await EvaluateOptionalSmartTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseLayerNodeAsync(XElement element, object? scope) =>
        new LayerNode(
            await EvaluateExpressionAttributeOrAsync(element, "opacity", 1f, scope),
            await ParseChildrenAsync(element, scope),
            await EvaluateOptionalSmartTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParsePositionedNodeAsync(XElement element, object? scope) =>
        new PositionedNode(
            new Point(
                await EvaluateRequiredExpressionAttributeAsync<int>(element, "x", scope),
                await EvaluateRequiredExpressionAttributeAsync<int>(element, "y", scope)),
            await EvaluateExpressionAttributeOrAsync(element, "anchorX", PositionAnchor.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "anchorY", PositionAnchor.Start, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "width", null, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "height", null, scope),
            await ParseChildrenAsync(element, scope),
            await EvaluateOptionalSmartTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseResizedNodeAsync(XElement element, object? scope)
    {
        List<Node> children = await ParseChildrenAsync(element, scope);
        return new ResizedNode(
            await EvaluateExpressionAttributeOrAsync(element, "scale", 1f, scope),
            await ParseOptionalSizeAsync(element, scope),
            await ParseResamplerTypeAsync(element, scope),
            children.Single(),
            await EvaluateOptionalSmartTemplateAttributeAsync(element, "key", scope));
    }

    private async Task<Node> ParseStackNodeAsync(XElement element, object? scope)
    {
        int spacing = await EvaluateExpressionAttributeOrAsync(element, "spacing", 0, scope);
        string directionRaw = await EvaluateRequiredTemplateAttributeAsync(element, "direction", scope);
        return new StackNode(
            directionRaw is "row" ? StackDirection.Row : StackDirection.Column,
            spacing,
            await EvaluateExpressionAttributeOrAsync(element, "runSpacing", spacing, scope),
            await EvaluateExpressionAttributeOrAsync(element, "wrap", false, scope),
            await EvaluateExpressionAttributeOrAsync(element, "justify", StackJustifyContent.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignItems", AlignItems.Start, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "width", null, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "height", null, scope),
            await ParseChildrenAsync(element, scope),
            await EvaluateOptionalSmartTemplateAttributeAsync(element, "key", scope));
    }

    private async Task<Node> ParseGridNodeAsync(XElement element, object? scope) =>
        new GridNode(
            await EvaluateExpressionAttributeOrAsync(element, "columns", 1, scope),
            await EvaluateExpressionAttributeOrAsync(element, "columnGap", 0, scope),
            await EvaluateExpressionAttributeOrAsync(element, "rowGap", 0, scope),
            await EvaluateExpressionAttributeOrAsync(element, "justifyItems", AlignItems.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignItems", AlignItems.Start, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "width", null, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "height", null, scope),
            await ParseChildrenAsync(element, scope),
            await EvaluateOptionalSmartTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseImageNodeAsync(XElement element, object? scope) =>
        new ImageNode(
            await EvaluateRequiredTemplateAttributeAsync(element, "namespace", scope),
            await EvaluateRequiredTemplateAttributeAsync(element, "id", scope),
            await EvaluateExpressionAttributeOrAsync(element, "colorBlending", PixelColorBlendingMode.Normal, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alphaComposition", PixelAlphaCompositionMode.SrcOver, scope),
            await EvaluateExpressionAttributeOrAsync(element, "repeat", 0, scope),
            await EvaluateOptionalSmartTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseTextNodeAsync(XElement element, object? scope) =>
        new TextNode(
            await EvaluateTemplateAttributeOrAsync(element, "value", string.Empty, scope),
            await EvaluateTemplateAttributeOrAsync(element, "fontFamily", "BoldFont", scope),
            await EvaluateExpressionAttributeOrAsync(element, "fontSize", 20, scope),
            await ParseColorAttributeOrAsync(element, "color", "#FFFFFF", scope),
            await EvaluateExpressionAttributeOrAsync(element, "align", TextAlignment.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignV", VerticalAlignment.Top, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignH", HorizontalAlignment.Left, scope),
            await ParseColorAttributeAsync(element, "strokeColor", scope),
            await EvaluateExpressionAttributeOrAsync(element, "strokeWidth", 0f, scope),
            await EvaluateExpressionAttributeOrAsync<float?>(element, "truncateWidth", null, scope),
            await EvaluateTemplateAttributeOrAsync(element, "truncateSuffix", string.Empty, scope),
            await EvaluateOptionalSmartTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseSetNodeAsync(XElement element, object? scope) =>
        new SetNode(
            GetRequiredAttributeValue(element, "var"),
            await EvaluateSetValueAsync(GetRequiredAttributeValue(element, "value"), scope),
            await EvaluateOptionalSmartTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseIfNodeAsync(XElement element, object? scope)
    {
        bool pass = await EvaluateRequiredExpressionAsAsync<bool>(GetRequiredAttributeValue(element, "rule"), scope);
        List<Node> children = pass ? await ParseChildrenAsync(element, scope) : [];
        return new LayerNode(1f, children, null);
    }

    private async Task<Node> ParseForNodeAsync(XElement element, object? scope)
    {
        string itemsExpr = GetRequiredAttributeValue(element, "items");
        IEnumerable<object>? items = await EvaluateCollectionAsync(itemsExpr, scope);
        if (items is null)
        {
            return new LayerNode(1f, [], null);
        }

        string varName = GetAttributeValueOrDefault(element, "var", "item");
        string indexName = GetAttributeValueOrDefault(element, "indexVar", "idx");
        List<Node> children = [];
        int index = 0;

        foreach (object item in items)
        {
            Dictionary<string, object?> childScope = MergeScope(scope, varName, item, indexName, index);
            children.AddRange(await ParseChildrenAsync(element, childScope));
            index++;
        }

        return new LayerNode(1f, children, null);
    }

    private async Task<Node> ParseIncludeNodeAsync(XElement element, object? scope)
    {
        string includePath = await ResolveIncludePathAsync(element, scope);
        return await LoadNodeFromPathAsync(includePath, scope);
    }

    private async Task<string> ResolveIncludePathAsync(XElement element, object? scope)
    {
        string relativePath = await EvaluateTemplateAsync(GetRequiredAttributeValue(element, "src"), scope);
        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }

        string currentBaseDir = _baseDirs.Peek();
        return Path.Combine(currentBaseDir, relativePath);
    }

    private async Task<List<Node>> ParseChildrenAsync(XElement element, object? scope)
    {
        List<Node> children = [];
        object? currentScope = scope;
        foreach (XElement child in element.Elements())
        {
            Node node = await ParseElementAsync(child, currentScope);
            if (node is SetNode setNode)
            {
                currentScope = MergeScope(currentScope, setNode.Name, setNode.Value);
                continue;
            }

            children.Add(node);
        }

        return children;
    }

    private async Task<object?> EvaluateSetValueAsync(string raw, object? scope)
    {
        if (raw.StartsWith("@{", StringComparison.Ordinal) && raw.EndsWith('}'))
        {
            return await _expressionEngine.EvalAsync(raw[2..^1], scope);
        }

        try
        {
            return await _expressionEngine.EvalAsync(raw, scope);
        }
        catch
        {
            return await EvaluateTemplateAsync(raw, scope);
        }
    }

    private async Task<IEnumerable<object>?> EvaluateCollectionAsync(string expression, object? scope)
    {
        object? rawItems = await _expressionEngine.EvalAsync(expression, scope);
        return rawItems switch
        {
            null => null,
            IEnumerable<object> objectItems => objectItems,
            IEnumerable enumerable => enumerable.Cast<object>(),
            IConvertible convertible => Enumerable.Range(0, Math.Max(0, Convert.ToInt32(convertible, CultureInfo.InvariantCulture)))
                .Cast<object>(),
            _ => null
        };
    }

    private static Dictionary<string, object?> MergeScope(object? parent, string variableName, object item, string indexName, int index)
    {
        Dictionary<string, object?> result = new(StringComparer.OrdinalIgnoreCase);
        if (parent is IDictionary<string, object?> map)
        {
            foreach ((string key, object? value) in map)
            {
                result[key] = value;
            }
        }

        result[variableName] = item;
        result[indexName] = index;
        return result;
    }

    private static Dictionary<string, object?> MergeScope(object? parent, string variableName, object? value)
    {
        Dictionary<string, object?> result = new(StringComparer.OrdinalIgnoreCase);
        if (parent is IDictionary<string, object?> map)
        {
            foreach ((string key, object? mapValue) in map)
            {
                result[key] = mapValue;
            }
        }

        result[variableName] = value;
        return result;
    }
}