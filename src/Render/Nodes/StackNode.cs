namespace Limekuma.Render.Nodes;

public sealed record StackNode(
    StackDirection Direction,
    int Spacing,
    List<Node> Children,
    string? Key
) : Node(Key);