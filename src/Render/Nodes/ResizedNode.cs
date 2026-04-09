using Limekuma.Render.Types;

namespace Limekuma.Render.Nodes;

public sealed record ResizedNode(
    float Scale,
    int? Width,
    int? Height,
    ResamplerType Resampler,
    Node Child,
    string? Key
) : Node(Key);
