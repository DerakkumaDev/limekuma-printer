using SixLabors.ImageSharp;

namespace Limekuma.Render;

public interface IMeasureService
{
    Size Measure(string text, string family, float size);
}