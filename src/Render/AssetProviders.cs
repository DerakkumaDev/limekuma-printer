using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace Limekuma.Render;

public sealed class AssetProvider : IAssetProvider, IMeasureService
{
    private readonly ConcurrentDictionary<string, Image> _assets;
    private readonly FontCollection _fontCollection;
    private readonly ConcurrentDictionary<string, string> _fontFamilyNames;
    private readonly Dictionary<string, (string, List<string>?)> _fontRules;
    private readonly Dictionary<string, (string, string?)> _pathRules;

    static AssetProvider() => Shared = new();

    public AssetProvider(string resourcePath = "./Resources/Resources.xml")
    {
        _fontRules = [];
        _pathRules = [];
        _assets = [];
        _fontCollection = new();
        _fontFamilyNames = [];
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
        if (!_fontRules.TryGetValue(key, out (string MainFont, List<string>? Fallbacks) font))
        {
            throw new FontFamilyNotFoundException(key);
        }

        FontFamily mainFont = LoadFont(font.MainFont);
        if (font.Fallbacks is null)
        {
            return (mainFont, []);
        }

        List<FontFamily> fallbacks = [.. font.Fallbacks.Select(LoadFont)];
        return (mainFont, fallbacks);
    }

    public Size Measure(string text, string family, float size)
    {
        (FontFamily fontFamily, List<FontFamily> fallbacks) = ResolveFont(family);
        Font font = fontFamily.CreateFont(size);
        FontRectangle rect = TextMeasurer.MeasureAdvance(text, new(font)
        {
            FallbackFontFamilies = fallbacks
        });
        return new((int)Math.Ceiling(rect.Width), (int)Math.Ceiling(rect.Height));
    }

    public string? GetPath(string key)
    {
        if (!_pathRules.TryGetValue(key, out (string Path, string? _) pathRule))
        {
            return null;
        }

        return pathRule.Path;
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
            string? key = node.Attribute("key")?.Value;
            string path = node.Value;
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            _pathRules[key] = new(path, node.Attribute("rule")?.Value);
        }
    }

    private void LoadFontRules(XElement resourcesNode)
    {
        foreach (XElement node in resourcesNode.Elements("FontFamily"))
        {
            string? key = node.Attribute("key")?.Value;
            string? fontPath = node.Element("Font")?.Value;
            if (string.IsNullOrEmpty(key))
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
                _fontRules[key] = new(fontPath, null);
                continue;
            }

            List<string> fallbackPaths =
                [.. fallbacks.Elements("Font").Select(e => e.Value).Where(p => !string.IsNullOrEmpty(p))];
            _fontRules[key] = new(fontPath, fallbackPaths);
        }
    }

    private string ResolveResourcePath(string ns, string key)
    {
        if (!_pathRules.TryGetValue(ns, out (string Path, string? Rule) pathRule))
        {
            return key;
        }

        if (pathRule.Rule is null)
        {
            return Path.Combine(pathRule.Path, key);
        }

        return Path.Combine(pathRule.Path, string.Format(pathRule.Rule, key));
    }

    private Image LoadImage(string path)
    {
        if (!_assets.TryGetValue(path, out Image? image))
        {
            image = Image.Load(path);
            _assets.TryAdd(path, image);
        }

        return image.Clone(_ => { });
    }

    private FontFamily LoadFont(string path)
    {
        if (_fontFamilyNames.TryGetValue(path, out string? fontName) &&
            _fontCollection.TryGet(fontName, out FontFamily fontFamily))
        {
            return fontFamily;
        }

        fontFamily = _fontCollection.Add(path);
        _fontFamilyNames.TryAdd(path, fontFamily.Name);
        return fontFamily;
    }
}