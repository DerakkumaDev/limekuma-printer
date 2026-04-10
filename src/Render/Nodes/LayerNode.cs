namespace Limekuma.Render.Nodes;

public sealed record LayerNode(
    float Opacity,
    IEnumerable<Node> Children,
    string? Key
) : Node(Key);
