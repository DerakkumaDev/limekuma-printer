using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Limekuma.Render;

public abstract record Node(string? Key);

public enum StackDirection
{
    Row,
    Column
}

public enum ResamplerType
{
    Bicubic,
    Box,
    CatmullRom,
    Hermite,
    Lanczos2,
    Lanczos3,
    Lanczos5,
    Lanczos8,
    MitchellNetravali,
    NearestNeighbor,
    Robidoux,
    RobidouxSharp,
    Spline,
    Triangle,
    Welch
}

public sealed record CanvasNode(
    int Width,
    int Height,
    Color? Background,
    List<Node> Children,
    string? Key
) : Node(Key);

public sealed record LayerNode(
    float Opacity,
    List<Node> Children,
    string? Key
) : Node(Key);

public sealed record PositionedNode(
    Point Position,
    List<Node> Children,
    string? Key
) : Node(Key);

public sealed record ResizedNode(
    float Scale,
    Size? DesiredSize,
    ResamplerType Resampler,
    Node Child,
    string? Key
) : Node(Key);

public sealed record StackNode(
    StackDirection Direction,
    int Spacing,
    List<Node> Children,
    string? Key
) : Node(Key);

public sealed record ImageNode(
    string Namespace,
    string ResourceKey,
    string? Key
) : Node(Key);

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