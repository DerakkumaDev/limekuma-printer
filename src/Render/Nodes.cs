using SixLabors.ImageSharp;

namespace Limekuma.Render;

public abstract record Node(string? Key = null);

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
    List<Node> Children,
    Color? Background = null,
    string? Key = null
) : Node(Key);

public sealed record LayerNode(
    List<Node> Children,
    float Opacity = 1f,
    string? Key = null
) : Node(Key);

public sealed record PositionedNode(
    Point Position,
    Node Child,
    string? Key = null
) : Node(Key);

public sealed record ResizedNode(
    Node Child,
    Size? DesiredSize = null,
    float Scale = 1f,
    ResamplerType Resampler = ResamplerType.Lanczos3,
    string? Key = null
) : Node(Key);

public sealed record StackNode(
    StackDirection Direction,
    int Spacing,
    List<Node> Children,
    string? Key = null
) : Node(Key);

public sealed record ImageNode(
    string Namespace,
    string ResourceKey,
    string? Key = null
) : Node(Key);

public sealed record TextNode(
    string Text,
    string FontFamily,
    float FontSize,
    Color Color,
    Color? StrokeColor = null,
    float? StrokeWidth = null,
    string? Key = null
) : Node(Key);