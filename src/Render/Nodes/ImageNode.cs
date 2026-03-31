using SixLabors.ImageSharp.PixelFormats;

namespace Limekuma.Render.Nodes;

public sealed record ImageNode(
    string Namespace,
    string ResourceKey,
    PixelColorBlendingMode ColorBlending,
    PixelAlphaCompositionMode AlphaComposition,
    int ForegroundRepeatCount,
    string? Key
) : Node(Key);