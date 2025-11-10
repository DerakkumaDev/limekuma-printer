using SixLabors.ImageSharp;

namespace Limekuma.Render.Nodes;

public sealed record ResizedNode(
    float Scale,
    Size? DesiredSize,
    ResamplerType Resampler,
    Node Child,
    string? Key
) : Node(Key);