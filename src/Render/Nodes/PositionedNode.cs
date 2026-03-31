using SixLabors.ImageSharp;
using Limekuma.Render.Types;

namespace Limekuma.Render.Nodes;

public sealed record PositionedNode(
    Point Position,
    PositionAnchor AnchorX,
    PositionAnchor AnchorY,
    int? Width,
    int? Height,
    List<Node> Children,
    string? Key
) : Node(Key);