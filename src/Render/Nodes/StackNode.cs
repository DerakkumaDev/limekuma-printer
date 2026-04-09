using Limekuma.Render.Types;

namespace Limekuma.Render.Nodes;

public sealed record StackNode(
    StackDirection Direction,
    float Spacing,
    float RunSpacing,
    bool Wrap,
    StackJustifyContent JustifyContent,
    AlignItems AlignItems,
    ContentAlign AlignContent,
    int? Width,
    int? Height,
    List<Node> Children,
    string? Key
) : Node(Key);
