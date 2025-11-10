using SixLabors.ImageSharp;

namespace Limekuma.Render.Nodes;

public sealed record PositionedNode(
    Point Position,
    List<Node> Children,
    string? Key
) : Node(Key);