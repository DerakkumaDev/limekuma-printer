using SixLabors.ImageSharp;

namespace Limekuma.Render.Nodes;

public sealed record CanvasNode(
    int Width,
    int Height,
    Color? Background,
    List<Node> Children,
    string? Key
) : Node(Key);