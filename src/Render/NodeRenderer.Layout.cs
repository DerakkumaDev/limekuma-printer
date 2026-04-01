using Limekuma.Render.Types;

namespace Limekuma.Render;

public static partial class NodeRenderer
{
    private static float ResolveMainAxisContainerSize(int? explicitSize, int? desiredSize, float fallback) =>
        explicitSize is { } value
            ? value
            : desiredSize is { } desired
                ? desired
                : fallback;

    private static (float Start, float Between) ResolveMainAxisLayout(StackJustifyContent justify, float containerMain,
        float contentMain, float baseSpacing, int itemCount)
    {
        float remaining = Math.Max(0, containerMain - contentMain);
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

    private static int ResolveStartCenterEndOffset(ContentAlign align, int containerSize, int itemSize)
    {
        int room = Math.Max(0, containerSize - itemSize);
        return align switch
        {
            ContentAlign.Center => room / 2,
            ContentAlign.End => room,
            _ => 0
        };
    }

    private static float ResolveStartCenterEndOffset(ContentAlign align, float containerSize, float itemSize)
    {
        float room = Math.Max(0, containerSize - itemSize);
        return align switch
        {
            ContentAlign.Center => room / 2,
            ContentAlign.End => room,
            _ => 0
        };
    }
}