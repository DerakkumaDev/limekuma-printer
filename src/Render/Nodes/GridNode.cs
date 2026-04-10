using Limekuma.Render.Types;

namespace Limekuma.Render.Nodes;

public sealed record GridNode(
    int Columns,
    int ColumnGap,
    int RowGap,
    ContentAlign JustifyContent,
    ContentAlign AlignContent,
    AlignItems JustifyItems,
    AlignItems AlignItems,
    int? Width,
    int? Height,
    IEnumerable<Node> Children,
    string? Key
) : Node(Key);
