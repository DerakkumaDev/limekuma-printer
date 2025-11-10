using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Limekuma.Render;

public interface IAssetProvider
{
    Image LoadImage(string resourceNamespace, string resourceKey);
    (FontFamily, List<FontFamily>) ResolveFont(string family);
}