using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Limekuma.Render.Nodes;

public sealed record TextNode(
    string Text,
    string FontFamily,
    int FontSize,
    Color Color,
    TextAlignment TextAlignment,
    VerticalAlignment VerticalAlignment,
    HorizontalAlignment HorizontalAlignment,
    Color? StrokeColor,
    float StrokeWidth,
    float? TruncateWidth,
    string TruncateSubfix,
    string? Key
) : Node(Key);