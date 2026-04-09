using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SmartFormat;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Limekuma.Render;

public sealed class AssetProvider
{
    private readonly ConcurrentDictionary<string, Image> _assets;
    private readonly ConcurrentDictionary<string, LoadedFont> _fontCache;
    private readonly Dictionary<string, (string, List<string>?)> _fontRules;
    private readonly Dictionary<string, (string, string?)> _pathRules;

    static AssetProvider() => Shared = new();

    public AssetProvider() : this("./Resources/Resources.xml")
    {
    }

    public AssetProvider(string resourcePath)
    {
        _fontRules = [];
        _pathRules = [];
        _assets = [];
        _fontCache = [];
        LoadResources(resourcePath);
    }

    public static AssetProvider Shared { get; }

    public Image LoadImage(string ns, string key)
    {
        string path = ResolveResourcePath(ns, key);
        return LoadImage(path);
    }

    public (FontFamily, List<FontFamily>) ResolveFont(string key)
    {
        if (!_fontRules.TryGetValue(key, out (string, List<string>?) font))
        {
            throw new FontFamilyNotFoundException(key);
        }

        (string mainFontName, List<string>? fallbackNames) = font;
        FontFamily mainFont = LoadFont(mainFontName);
        if (fallbackNames is null)
        {
            return (mainFont, []);
        }

        List<FontFamily> fallbacks = [.. fallbackNames.Select(LoadFont)];
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

    private void LoadResources(string resourcePath)
    {
        XDocument doc = XDocument.Load(resourcePath);
        XElement? resources = doc.Element("Resources");
        if (resources is null)
        {
            return;
        }

        LoadPathRules(resources);
        LoadFontRules(resources);
    }

    private void LoadPathRules(XElement resourcesNode)
    {
        foreach (XElement node in resourcesNode.Elements("Path"))
        {
            string? ns = node.Attribute("namespace")?.Value;
            string path = node.Value;
            if (string.IsNullOrEmpty(ns))
            {
                continue;
            }

            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            _pathRules[ns] = new(path, node.Attribute("rule")?.Value);
        }
    }

    private void LoadFontRules(XElement resourcesNode)
    {
        foreach (XElement node in resourcesNode.Elements("FontFamily"))
        {
            string? ns = node.Attribute("namespace")?.Value;
            string? fontPath = node.Element("Font")?.Value;
            if (string.IsNullOrEmpty(ns))
            {
                continue;
            }

            if (string.IsNullOrEmpty(fontPath))
            {
                continue;
            }

            XElement? fallbacks = node.Element("Fallbacks");
            if (fallbacks is null)
            {
                _fontRules[ns] = new(fontPath, null);
                continue;
            }

            List<string> fallbackPaths =
                [.. fallbacks.Elements("Font").Select(e => e.Value).Where(p => !string.IsNullOrEmpty(p))];
            _fontRules[ns] = new(fontPath, fallbackPaths);
        }
    }

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
