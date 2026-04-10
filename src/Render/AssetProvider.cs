using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SmartFormat;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Limekuma.Render;

public sealed class AssetProvider
{
    private readonly ConcurrentDictionary<string, Image> _assets;
    private readonly ConcurrentDictionary<string, LoadedFont> _fontCache;
    private readonly ConcurrentDictionary<string, ImmutableArray<FontFamily>> _fontFallbackCache;
    private readonly FrozenDictionary<string, (string, ImmutableArray<string>)> _fontRules;
    private readonly FrozenDictionary<string, (string, string?)> _pathRules;

    static AssetProvider() => Shared = new();

    public AssetProvider() : this("./Resources/Resources.xml")
    {
    }

    public AssetProvider(string resourcePath)
    {
        _assets = [];
        _fontCache = [];
        _fontFallbackCache = [];
        (_pathRules, _fontRules) = LoadResources(resourcePath);
    }

    public static AssetProvider Shared { get; }

    public Image LoadImage(string ns, string key)
    {
        string path = ResolveResourcePath(ns, key);
        return LoadImage(path);
    }

    public (FontFamily, ImmutableArray<FontFamily>) ResolveFont(string key)
    {
        if (!_fontRules.TryGetValue(key, out (string, ImmutableArray<string>) font))
        {
            throw new FontFamilyNotFoundException(key);
        }

        (string mainFontName, ImmutableArray<string> fallbackNames) = font;
        FontFamily mainFont = LoadFont(mainFontName);
        if (fallbackNames.IsDefaultOrEmpty)
        {
            return (mainFont, []);
        }

        ImmutableArray<FontFamily> fallbacks =
            _fontFallbackCache.GetOrAdd(key, _ => [.. fallbackNames.Select(LoadFont)]);
        return (mainFont, fallbacks);
    }

    public string? GetPath(string ns)
    {
        if (!_pathRules.TryGetValue(ns, out (string, string?) pathRule))
        {
            return null;
        }

        (string path, _) = pathRule;
        return path;
    }

    private static (FrozenDictionary<string, (string, string?)>,
        FrozenDictionary<string, (string, ImmutableArray<string>)>) LoadResources(string resourcePath)
    {
        XDocument doc = XDocument.Load(resourcePath);
        XElement? resources = doc.Element("Resources");
        if (resources is null)
        {
            return (FrozenDictionary<string, (string, string?)>.Empty,
                FrozenDictionary<string, (string, ImmutableArray<string>)>.Empty);
        }

        return (LoadPathRules(resources), LoadFontRules(resources));
    }

    private static FrozenDictionary<string, (string, string?)> LoadPathRules(XElement resourcesNode) => resourcesNode
        .Elements("Path").Select(node => new
        {
            Namespace = node.Attribute("namespace")?.Value,
            Path = node.Value,
            Rule = node.Attribute("rule")?.Value
        }).Where(x => !string.IsNullOrEmpty(x.Namespace) && !string.IsNullOrEmpty(x.Path))
        .ToFrozenDictionary(x => x.Namespace!, x => (x.Path, x.Rule));

    private static FrozenDictionary<string, (string, ImmutableArray<string>)> LoadFontRules(XElement resourcesNode) =>
        resourcesNode.Elements("FontFamily").Select(node =>
            {
                string? fontPath = node.Element("Font")?.Value;
                ImmutableArray<string> fallbackPaths =
                [
                    .. node.Element("Fallbacks")?.Elements("Font").Select(e => e.Value)
                        .Where(p => !string.IsNullOrEmpty(p)) ?? []
                ];
                return new
                {
                    Namespace = node.Attribute("namespace")?.Value,
                    FontPath = fontPath,
                    Fallbacks = fallbackPaths
                };
            }).Where(x => !string.IsNullOrEmpty(x.Namespace) && !string.IsNullOrEmpty(x.FontPath))
            .ToFrozenDictionary(x => x.Namespace!, x => (x.FontPath!, x.Fallbacks));

    private string ResolveResourcePath(string ns, string key)
    {
        if (!_pathRules.TryGetValue(ns, out (string, string?) pathRule))
        {
            return key;
        }

        (string path, string? rule) = pathRule;
        return Path.Combine(path, rule is null ? key : Smart.Format(rule, new { key }));
    }

    private Image LoadImage(string path)
    {
        Image loaded = _assets.GetOrAdd(path, _ =>
        {
            Image image = Image.Load(path);
            return image;
        });

        return loaded;
    }

    private FontFamily LoadFont(string path)
    {
        LoadedFont loaded = _fontCache.GetOrAdd(path, p =>
        {
            FontCollection collection = new();
            FontFamily family = collection.Add(p);
            return new(collection, family);
        });
        return loaded.Family;
    }

    private sealed record LoadedFont(FontCollection Collection, FontFamily Family);
}
