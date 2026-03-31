using Limekuma.Render.Types;

namespace Limekuma.Render.Nodes;

public sealed record GridNode(
    int Columns,
    int ColumnGap,
    int RowGap,
    AlignItems JustifyItems,
    AlignItems AlignItems,
    int? Width,
    int? Height,
    List<Node> Children,
    string? Key
) : Node(Key);