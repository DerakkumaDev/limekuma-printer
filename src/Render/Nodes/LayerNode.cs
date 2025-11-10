namespace Limekuma.Render.Nodes;

public sealed record LayerNode(
    float Opacity,
    List<Node> Children,
    string? Key
) : Node(Key);