using Limekuma.Render.Nodes;
using Limekuma.Render.Types;
using SixLabors.Fonts;
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
            await EvaluateOptionalTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseLayerNodeAsync(XElement element, object? scope) =>
        new LayerNode(
            await EvaluateExpressionAttributeOrAsync(element, "opacity", 1f, scope),
            await ParseChildrenAsync(element, scope),
            await EvaluateOptionalTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParsePositionedNodeAsync(XElement element, object? scope) =>
        new PositionedNode(
            new(
                await EvaluateRequiredExpressionAttributeAsync<int>(element, "x", scope),
                await EvaluateRequiredExpressionAttributeAsync<int>(element, "y", scope)),
            await EvaluateExpressionAttributeOrAsync(element, "anchorX", PositionAnchor.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "anchorY", PositionAnchor.Start, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "width", null, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "height", null, scope),
            await ParseChildrenAsync(element, scope),
            await EvaluateOptionalTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseResizedNodeAsync(XElement element, object? scope)
    {
        List<Node> children = await ParseChildrenAsync(element, scope);
        if (children.Count is not 1)
        {
            throw new InvalidOperationException(
                $"DSL[ResizedChildCount] Invalid child count. Context: element='Resized', expected='1', actual='{children.Count}'");
        }

        return new ResizedNode(
            await EvaluateExpressionAttributeOrAsync(element, "scale", 1f, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "width", null, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "height", null, scope),
            await ParseResamplerTypeAsync(element, scope),
            children[0],
            await EvaluateOptionalTemplateAttributeAsync(element, "key", scope));
    }

    private async Task<Node> ParseStackNodeAsync(XElement element, object? scope)
    {
        float spacing = await EvaluateExpressionAttributeOrAsync(element, "spacing", 0f, scope);
        return new StackNode(
            await EvaluateRequiredExpressionAttributeAsync<StackDirection>(element, "direction", scope),
            spacing,
            await EvaluateExpressionAttributeOrAsync(element, "runSpacing", spacing, scope),
            await EvaluateExpressionAttributeOrAsync(element, "wrap", false, scope),
            await EvaluateExpressionAttributeOrAsync(element, "justify", StackJustifyContent.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignItems", AlignItems.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignContent", ContentAlign.Start, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "width", null, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "height", null, scope),
            await ParseChildrenAsync(element, scope),
            await EvaluateOptionalTemplateAttributeAsync(element, "key", scope));
    }

    private async Task<Node> ParseGridNodeAsync(XElement element, object? scope) =>
        new GridNode(
            await EvaluateExpressionAttributeOrAsync(element, "columns", 1, scope),
            await EvaluateExpressionAttributeOrAsync(element, "columnGap", 0, scope),
            await EvaluateExpressionAttributeOrAsync(element, "rowGap", 0, scope),
            await EvaluateExpressionAttributeOrAsync(element, "justifyContent", ContentAlign.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignContent", ContentAlign.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "justifyItems", AlignItems.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignItems", AlignItems.Start, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "width", null, scope),
            await EvaluateExpressionAttributeOrAsync<int?>(element, "height", null, scope),
            await ParseChildrenAsync(element, scope),
            await EvaluateOptionalTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseImageNodeAsync(XElement element, object? scope) =>
        new ImageNode(
            await EvaluateRequiredTemplateAttributeAsync(element, "namespace", scope),
            await EvaluateRequiredTemplateAttributeAsync(element, "id", scope),
            await EvaluateExpressionAttributeOrAsync(element, "colorBlending", PixelColorBlendingMode.Normal, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alphaComposition", PixelAlphaCompositionMode.SrcOver,
                scope),
            await EvaluateExpressionAttributeOrAsync(element, "repeat", 0, scope),
            await EvaluateOptionalTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseTextNodeAsync(XElement element, object? scope) =>
        new TextNode(
            await EvaluateTemplateAttributeOrAsync(element, "value", string.Empty, scope),
            await EvaluateTemplateAttributeOrAsync(element, "fontFamily", "BoldFont", scope),
            await EvaluateExpressionAttributeOrAsync(element, "fontSize", 20f, scope),
            await ParseColorAttributeOrAsync(element, "color", "#FFFFFF", scope),
            await EvaluateExpressionAttributeOrAsync(element, "align", TextAlignment.Start, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignV", VerticalAlignment.Top, scope),
            await EvaluateExpressionAttributeOrAsync(element, "alignH", HorizontalAlignment.Left, scope),
            await ParseColorAttributeAsync(element, "strokeColor", scope),
            await EvaluateExpressionAttributeOrAsync(element, "strokeWidth", 0f, scope),
            await EvaluateExpressionAttributeOrAsync<float?>(element, "truncateWidth", null, scope),
            await EvaluateTemplateAttributeOrAsync(element, "truncateSuffix", string.Empty, scope),
            await EvaluateOptionalTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseSetNodeAsync(XElement element, object? scope) =>
        new SetNode(
            GetRequiredAttributeValue(element, "var"),
            await EvaluateRequiredExpressionAttributeAsync<object>(element, "value", scope),
            await EvaluateOptionalTemplateAttributeAsync(element, "key", scope));

    private async Task<Node> ParseIfNodeAsync(XElement element, object? scope)
    {
        bool pass = await EvaluateRequiredExpressionAsAsync<bool>(GetRequiredAttributeValue(element, "rule"), scope);
        List<Node> children = pass ? await ParseChildrenAsync(element, scope) : [];
        return new LayerNode(1f, children, null);
    }

    private Task<Node> ParseElseIfNodeAsync(XElement element, object? scope) => throw new InvalidOperationException(
        $"DSL[ConditionalOrphan] ElseIf must follow an If or ElseIf at the same level. Context: parent='{element.Parent?.Name.LocalName ?? "null"}'");

    private Task<Node> ParseElseNodeAsync(XElement element, object? scope) => throw new InvalidOperationException(
        $"DSL[ConditionalOrphan] Else must follow an If or ElseIf at the same level. Context: parent='{element.Parent?.Name.LocalName ?? "null"}'");

    private async Task<Node> ParseForNodeAsync(XElement element, object? scope)
    {
        string itemsExpr = GetRequiredAttributeValue(element, "items");
        IEnumerable<object> items = await EvaluateCollectionAsync(itemsExpr, scope);

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
        (string src, string includePath, string currentPath, string rootPath) =
            await ResolveIncludePathAsync(element, scope);
        if (!File.Exists(includePath))
        {
            throw new InvalidOperationException(
                $"DSL[IncludeNotFound] Include file not found. Context: src='{src}', from='{GetDisplayPath(currentPath, rootPath)}', resolved='{GetDisplayPath(includePath, rootPath)}', root='{GetDisplayPath(rootPath, rootPath)}'");
        }

        try
        {
            Node includeNode = await LoadNodeFromPathAsync(includePath, scope);
            if (element.Parent is not null && includeNode is CanvasNode)
            {
                throw new InvalidOperationException(
                    $"DSL[CanvasNested] Canvas can only be used as layout root. Context: parent='{element.Parent.Name.LocalName}', include='{src}'");
            }

            return includeNode;
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("DSL[IncludeCircular]",
                                                       StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"DSL[IncludeCircular] Circular include detected while resolving include. Context: src='{src}', from='{GetDisplayPath(currentPath, rootPath)}'. Cause: {ex.Message}",
                ex);
        }
    }

    private async Task<(string Src, string FullPath, string CurrentPath, string RootPath)> ResolveIncludePathAsync(
        XElement element, object? scope)
    {
        string relativePath = await EvaluateTemplateAsync(GetRequiredAttributeValue(element, "src"), scope);
        if (_baseDirs.Count is 0)
        {
            throw new InvalidOperationException(
                "DSL[IncludeBaseDir] Include base directory is not available. Context: baseDirStack='empty'");
        }

        string currentBaseDir = _baseDirs.Peek();
        string rootBaseDir = _baseDirs.ToArray()[^1];
        string currentPath = _loadingPathStack.Count > 0 ? _loadingPathStack.Peek() : currentBaseDir;
        string candidatePath =
            Path.IsPathRooted(relativePath) ? relativePath : Path.Combine(currentBaseDir, relativePath);
        string fullIncludePath = Path.GetFullPath(candidatePath);
        string fullRootPath = Path.GetFullPath(rootBaseDir);
        if (IsPathUnderRoot(fullIncludePath, fullRootPath))
        {
            return (relativePath, fullIncludePath, currentPath, fullRootPath);
        }

        string resolvedRelativeToRoot =
            NormalizePathForDisplay(Path.GetRelativePath(fullRootPath, fullIncludePath));
        throw new InvalidOperationException(
            $"DSL[IncludeOutsideRoot] Include path is outside layout root. Context: src='{relativePath}', from='{GetDisplayPath(currentPath, fullRootPath)}', candidate='{NormalizePathForDisplay(candidatePath)}', resolved='{resolvedRelativeToRoot}', root='{GetDisplayPath(fullRootPath, fullRootPath)}'");
    }

    private static bool IsPathUnderRoot(string path, string root)
    {
        StringComparison comparison =
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        string normalizedRoot = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        return path.Equals(root, comparison) || path.StartsWith(normalizedRoot, comparison);
    }

    private async Task<List<Node>> ParseChildrenAsync(XElement element, object? scope)
    {
        List<Node> children = [];
        object? currentScope = scope;
        List<XElement> childElements = [.. element.Elements()];
        for (int i = 0; i < childElements.Count; i++)
        {
            XElement child = childElements[i];
            if (child.Name.LocalName.Equals("Canvas", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"DSL[CanvasNested] Canvas can only be used as layout root. Context: parent='{element.Name.LocalName}'");
            }

            Node node;
            if (child.Name.LocalName.Equals("If", StringComparison.Ordinal))
            {
                (node, i) = await ParseConditionalChainAsync(childElements, i, currentScope);
            }
            else if (child.Name.LocalName.Equals("ElseIf", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"DSL[ConditionalOrphan] ElseIf must follow an If or ElseIf at the same level. Context: parent='{element.Name.LocalName}'");
            }
            else if (child.Name.LocalName.Equals("Else", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"DSL[ConditionalOrphan] Else must follow an If or ElseIf at the same level. Context: parent='{element.Name.LocalName}'");
            }
            else
            {
                node = await ParseElementAsync(child, currentScope);
            }

            if (node is SetNode setNode)
            {
                currentScope = MergeScope(currentScope, setNode.Name, setNode.Value);
                continue;
            }

            children.Add(node);
        }

        return children;
    }

    private async Task<(Node Node, int LastConsumedIndex)> ParseConditionalChainAsync(List<XElement> siblings,
        int index, object? scope)
    {
        XElement ifElement = siblings[index];
        bool branchMatched =
            await EvaluateRequiredExpressionAsAsync<bool>(GetRequiredAttributeValue(ifElement, "rule"), scope);
        List<Node> selectedChildren = branchMatched ? await ParseChildrenAsync(ifElement, scope) : [];
        bool hasElse = false;

        int cursor = index + 1;
        while (cursor < siblings.Count)
        {
            XElement branchElement = siblings[cursor];
            string tagName = branchElement.Name.LocalName;
            if (tagName.Equals("ElseIf", StringComparison.Ordinal))
            {
                if (hasElse)
                {
                    throw new InvalidOperationException(
                        $"DSL[ConditionalOrder] ElseIf can not appear after Else. Context: parent='{branchElement.Parent?.Name.LocalName ?? "null"}'");
                }

                if (!branchMatched)
                {
                    bool elseIfPass =
                        await EvaluateRequiredExpressionAsAsync<bool>(GetRequiredAttributeValue(branchElement, "rule"),
                            scope);
                    if (elseIfPass)
                    {
                        branchMatched = true;
                        selectedChildren = await ParseChildrenAsync(branchElement, scope);
                    }
                }

                cursor++;
                continue;
            }

            if (tagName.Equals("Else", StringComparison.Ordinal))
            {
                if (hasElse)
                {
                    throw new InvalidOperationException(
                        $"DSL[ConditionalOrder] Else can only appear once in a conditional chain. Context: parent='{branchElement.Parent?.Name.LocalName ?? "null"}'");
                }

                hasElse = true;
                if (!branchMatched)
                {
                    branchMatched = true;
                    selectedChildren = await ParseChildrenAsync(branchElement, scope);
                }

                cursor++;
                continue;
            }

            break;
        }

        return (new LayerNode(1f, selectedChildren, null), cursor - 1);
    }

    private async Task<IEnumerable<object>> EvaluateCollectionAsync(string expression, object? scope)
    {
        object? rawItems = await _expressionEngine.EvalAsync(expression, scope);
        return rawItems switch
        {
            null => throw BuildInvalidForItemsException(expression, null, "resolved to null"),
            IEnumerable<object> objectItems => objectItems,
            IEnumerable enumerable => enumerable.Cast<object>(),
            IConvertible convertible => BuildRangeItems(convertible, expression),
            _ => throw BuildInvalidForItemsException(expression, rawItems, "is not IEnumerable or integer-like")
        };
    }

    private static IEnumerable<object> BuildRangeItems(IConvertible value, string expression)
    {
        if (value is bool)
        {
            throw BuildInvalidForItemsException(expression, value, "bool is not allowed as loop count");
        }

        decimal numericValue;
        try
        {
            numericValue = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw BuildInvalidForItemsException(expression, value, "can not convert to number", ex);
        }

        if (numericValue < 0)
        {
            throw BuildInvalidForItemsException(expression, value, "must be >= 0 when used as loop count");
        }

        if (decimal.Truncate(numericValue) != numericValue)
        {
            throw BuildInvalidForItemsException(expression, value, "must be an integer when used as loop count");
        }

        if (numericValue > int.MaxValue)
        {
            throw BuildInvalidForItemsException(expression, value,
                $"is too large. Max allowed loop count is {int.MaxValue}");
        }

        int count = decimal.ToInt32(numericValue);
        return Enumerable.Range(0, count).Cast<object>();
    }

    private static InvalidOperationException BuildInvalidForItemsException(string expression, object? value,
        string reason,
        Exception? innerException = null)
    {
        string typeName = value?.GetType().FullName ?? "null";
        string valueText = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null";
        return new(
            $"DSL[ForItems] Invalid For.items expression. Context: expression='{expression}', reason='{reason}', actualType='{typeName}', actualValue='{valueText}', expected='IEnumerable or non-negative integer'",
            innerException);
    }

    private static Dictionary<string, object?> MergeScope(object? parent, string variableName, object item,
        string indexName, int index)
    {
        Dictionary<string, object?> result = new(StringComparer.OrdinalIgnoreCase);
        CopyParentScope(parent, result);

        result[variableName] = item;
        result[indexName] = index;
        return result;
    }

    private static Dictionary<string, object?> MergeScope(object? parent, string variableName, object? value)
    {
        Dictionary<string, object?> result = new(StringComparer.OrdinalIgnoreCase);
        CopyParentScope(parent, result);

        result[variableName] = value;
        return result;
    }

    private static void CopyParentScope(object? parent, Dictionary<string, object?> target)
    {
        if (parent is not IDictionary dictionary)
        {
            return;
        }

        foreach (DictionaryEntry entry in dictionary)
        {
            if (entry.Key is not string key)
            {
                continue;
            }

            target[key] = entry.Value;
        }
    }
}
