namespace Limekuma.Render.Nodes;

public sealed record ImageNode(
    string Namespace,
    string ResourceKey,
    string? Key
) : Node(Key);