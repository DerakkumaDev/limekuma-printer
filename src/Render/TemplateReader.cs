using Limekuma.Render.ExpressionEngine;
using Limekuma.Render.Nodes;
using SmartFormat;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Limekuma.Render;

public sealed partial class TemplateReader
{
    private static readonly SmartFormatter Formatter = Smart.CreateDefaultSmartFormat();
    private readonly Stack<string> _baseDirs = new();
    private readonly AsyncNCalcEngine _expressionEngine;

    private readonly HashSet<string> _loadingPathSet =
        new(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

    private readonly Stack<string> _loadingPathStack = new();
    private readonly Dictionary<string, Func<XElement, object?, Task<Node>>> _nodeParsers;

    public TemplateReader(AsyncNCalcEngine expressionEngine)
    {
        _expressionEngine = expressionEngine;
        _nodeParsers = CreateNodeParsers();
    }

    private Dictionary<string, Func<XElement, object?, Task<Node>>> CreateNodeParsers() =>
        new(StringComparer.Ordinal)
        {
            ["Canvas"] = ParseCanvasNodeAsync,
            ["Layer"] = ParseLayerNodeAsync,
            ["Positioned"] = ParsePositionedNodeAsync,
            ["Resized"] = ParseResizedNodeAsync,
            ["Stack"] = ParseStackNodeAsync,
            ["Grid"] = ParseGridNodeAsync,
            ["Image"] = ParseImageNodeAsync,
            ["Text"] = ParseTextNodeAsync,
            ["Set"] = ParseSetNodeAsync,
            ["If"] = ParseIfNodeAsync,
            ["ElseIf"] = ParseElseIfNodeAsync,
            ["Else"] = ParseElseNodeAsync,
            ["For"] = ParseForNodeAsync,
            ["Include"] = ParseIncludeNodeAsync
        };

    public async Task<Node> LoadAsync(string xmlPath, object? scope)
    {
        string fullPath = Path.GetFullPath(xmlPath);
        Node root = await LoadNodeFromPathAsync(fullPath, scope);
        if (root is CanvasNode)
        {
            return root;
        }

        string actualRoot = root.GetType().Name.Replace("Node", string.Empty, StringComparison.Ordinal);
        throw new InvalidOperationException(
            $"DSL[ParseRoot] Invalid layout root node. Context: expected='Canvas', actual='{actualRoot}', path='{GetDisplayPath(fullPath)}'");
    }

    private async Task<Node> LoadNodeFromPathAsync(string xmlPath, object? scope)
    {
        string fullPath = Path.GetFullPath(xmlPath);
        if (!_loadingPathSet.Add(fullPath))
        {
            string fromPath = _loadingPathStack.Count > 0 ? _loadingPathStack.Peek() : fullPath;
            List<string> chain =
                [.. _loadingPathStack.Reverse().Select(path => GetDisplayPath(path)), GetDisplayPath(fullPath)];
            throw new InvalidOperationException(
                $"DSL[IncludeCircular] Circular include detected. Context: include='{GetDisplayPath(fullPath)}', from='{GetDisplayPath(fromPath)}', chain='{string.Join(" -> ", chain)}'");
        }

        _loadingPathStack.Push(fullPath);
        try
        {
            return await ExecuteInBaseDirAsync(fullPath, async () =>
            {
                XDocument document = XDocument.Load(fullPath);
                return await ParseRootAsync(document, scope);
            });
        }
        finally
        {
            _loadingPathStack.Pop();
            _loadingPathSet.Remove(fullPath);
        }
    }

    private async Task<Node> ParseRootAsync(XDocument document, object? scope)
    {
        XElement root = document.Root ??
                        throw new InvalidOperationException("DSL[XmlRoot] Missing XML root node. Context: root='null'");
        return await ParseElementAsync(root, scope);
    }

    private async Task<T> ExecuteInBaseDirAsync<T>(string path, Func<Task<T>> action)
    {
        string? baseDir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrWhiteSpace(baseDir))
        {
            _baseDirs.Push(baseDir);
        }

        try
        {
            return await action();
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(baseDir))
            {
                _baseDirs.Pop();
            }
        }
    }

    private async Task<Node> ParseElementAsync(XElement element, object? scope)
    {
        if (!_nodeParsers.TryGetValue(element.Name.LocalName, out Func<XElement, object?, Task<Node>>? parser))
        {
            throw new InvalidOperationException(
                $"DSL[UnknownTag] Unknown tag. Context: tag='{element.Name.LocalName}'");
        }

        return await parser(element, scope);
    }

    private static string GetRequiredAttributeValue(XElement element, string name) => element.Attribute(name)?.Value ??
        throw new InvalidOperationException(
            $"DSL[MissingAttribute] Missing required attribute. Context: element='{element.Name.LocalName}', attribute='{name}'");

    private static string GetAttributeValueOrDefault(XElement element, string name, string defaultValue) =>
        element.Attribute(name)?.Value ?? defaultValue;

    private string GetDisplayPath(string path, string? rootPath = null)
    {
        string fullPath = Path.GetFullPath(path);
        string? root = rootPath;
        if (string.IsNullOrWhiteSpace(root) && _baseDirs.Count > 0)
        {
            root = _baseDirs.ToArray()[^1];
        }

        if (string.IsNullOrWhiteSpace(root))
        {
            return NormalizePathForDisplay(fullPath);
        }

        string fullRoot = Path.GetFullPath(root);
        string relativePath = Path.GetRelativePath(fullRoot, fullPath);
        return NormalizePathForDisplay(relativePath);
    }

    private static string NormalizePathForDisplay(string path) => path.Replace('\\', '/');

    [GeneratedRegex(@"@\{([^{}]+)\}")]
    private static partial Regex ExprSegmentRegex();
}
