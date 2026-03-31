using Limekuma.Render.ExpressionEngine;
using Limekuma.Render.Nodes;
using SmartFormat;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Limekuma.Render;

public sealed partial class TemplateReader
{
    private static readonly SmartFormatter Formatter = Smart.CreateDefaultSmartFormat();
    private readonly AsyncNCalcEngine _expressionEngine;
    private readonly Stack<string> _baseDirs = new();
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
            ["For"] = ParseForNodeAsync,
            ["Include"] = ParseIncludeNodeAsync
        };

    public async Task<Node> LoadAsync(string xmlPath, object? scope)
    {
        string fullPath = Path.GetFullPath(xmlPath);
        return await LoadNodeFromPathAsync(fullPath, scope);
    }

    private async Task<Node> LoadNodeFromPathAsync(string xmlPath, object? scope) =>
        await ExecuteInBaseDirAsync(xmlPath, async () =>
        {
            XDocument document = XDocument.Load(xmlPath);
            return await ParseRootAsync(document, scope);
        });

    private async Task<Node> ParseRootAsync(XDocument document, object? scope)
    {
        XElement root = document.Root ?? throw new InvalidOperationException("XML root node is required");
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
            throw new NotSupportedException($"Unknown tag: {element.Name}");
        }

        return await parser(element, scope);
    }

    private static string GetRequiredAttributeValue(XElement element, string name) =>
        element.Attribute(name)?.Value ?? throw new InvalidOperationException($"Missing required attribute: {name}");

    private static string GetAttributeValueOrDefault(XElement element, string name, string defaultValue) =>
        element.Attribute(name)?.Value ?? defaultValue;

    [GeneratedRegex(@"@\{([^{}]+)\}")]
    private static partial Regex ExprSegmentRegex();
}