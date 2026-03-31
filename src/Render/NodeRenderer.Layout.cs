using Limekuma.Render.Types;

namespace Limekuma.Render;

public static partial class NodeRenderer
{
    private static (int Start, int Between) ResolveMainAxisLayout(StackJustifyContent justify, int containerMain,
        int contentMain, int baseSpacing, int itemCount)
    {
        int remaining = Math.Max(0, containerMain - contentMain);
        return justify switch
        {
            StackJustifyContent.Start => (0, baseSpacing),
            StackJustifyContent.Center => (remaining / 2, baseSpacing),
            StackJustifyContent.End => (remaining, baseSpacing),
            StackJustifyContent.SpaceBetween when itemCount > 1 => (0, baseSpacing + remaining / (itemCount - 1)),
            StackJustifyContent.SpaceAround when itemCount > 0 =>
                (remaining / (itemCount * 2), baseSpacing + remaining / itemCount),
            StackJustifyContent.SpaceEvenly when itemCount > 0 =>
                (remaining / (itemCount + 1), baseSpacing + remaining / (itemCount + 1)),
            _ => (0, baseSpacing)
        };
    }

    private static int ResolveStartCenterEndOffset(AlignItems alignItems, int containerSize, int itemSize)
    {
        int room = Math.Max(0, containerSize - itemSize);
        return alignItems switch
        {
            AlignItems.Center => room / 2,
            AlignItems.End => room,
            _ => 0
        };
    }
}