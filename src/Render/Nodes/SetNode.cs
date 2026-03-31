namespace Limekuma.Render.Nodes;

public sealed record SetNode(
    string Name,
    object? Value,
    string? Key
) : Node(Key);