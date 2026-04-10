using Limekuma.Render.Nodes;
using Limekuma.Render.Types;
using SixLabors.ImageSharp;

namespace Limekuma.Render;

public static partial class NodeRenderer
{
    private static float ResolveMainAxisContainerSize(int? explicitSize, int? desiredSize, float fallback) =>
        explicitSize ?? desiredSize ?? fallback;

    private static List<List<(Node Node, Size Size)>> ResolveStackLines(IEnumerable<(Node Node, Size Size)> items,
        bool isRow, bool wrap, float spacing, float containerMain)
    {
        List<List<(Node Node, Size Size)>> lines = [];
        List<(Node Node, Size Size)> currentLine = [];
        float currentMain = 0;
        foreach ((Node node, Size size) in items)
        {
            float itemMain = isRow ? size.Width : size.Height;
            float nextMain = currentLine.Count is 0 ? itemMain : currentMain + spacing + itemMain;
            bool wrapNow = wrap && currentLine.Count > 0 && nextMain > containerMain;
            if (wrapNow)
            {
                lines.Add(currentLine);
                currentLine = [];
                currentMain = 0;
            }

            currentLine.Add((node, size));
            currentMain = currentLine.Count is 1 ? itemMain : currentMain + spacing + itemMain;
        }

        if (currentLine.Count > 0)
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    private static List<(float Main, int Cross)> ResolveStackLineSizes(
        IEnumerable<IReadOnlyList<(Node Node, Size Size)>> lines, bool isRow, float spacing)
    {
        List<(float Main, int Cross)> lineSizes = [];
        foreach (IReadOnlyList<(Node Node, Size Size)> line in lines)
        {
            float lineMain = line.Sum(i => isRow ? i.Size.Width : i.Size.Height);
            if (line.Count > 1)
            {
                lineMain += spacing * (line.Count - 1);
            }

            int lineCross = line.Max(i => isRow ? i.Size.Height : i.Size.Width);
            lineSizes.Add((lineMain, lineCross));
        }

        return lineSizes;
    }

    private static (float Main, float Cross) ResolveStackContentSize(List<(float Main, int Cross)> lineSizes,
        float runSpacing)
    {
        if (lineSizes.Count is 0)
        {
            return (0, 0);
        }

        float contentMain = lineSizes.Max(l => l.Main);
        float contentCross = lineSizes.Sum(l => l.Cross) + (Math.Max(0, lineSizes.Count - 1) * runSpacing);
        return (contentMain, contentCross);
    }

    private static (float Start, float Between) ResolveMainAxisLayout(StackJustifyContent justify, float containerMain,
        float contentMain, float baseSpacing, int itemCount)
    {
        float remaining = containerMain - contentMain;
        float distributable = Math.Max(0, remaining);
        return justify switch
        {
            StackJustifyContent.Start => (0, baseSpacing),
            StackJustifyContent.Center => (remaining / 2, baseSpacing),
            StackJustifyContent.End => (remaining, baseSpacing),
            StackJustifyContent.SpaceBetween when itemCount > 1 => (0, baseSpacing + (distributable / (itemCount - 1))),
            StackJustifyContent.SpaceAround when itemCount > 0 => (distributable / (itemCount * 2),
                baseSpacing + (distributable / itemCount)),
            StackJustifyContent.SpaceEvenly when itemCount > 0 => (distributable / (itemCount + 1),
                baseSpacing + (distributable / (itemCount + 1))),
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
        float room = containerSize - itemSize;
        return align switch
        {
            ContentAlign.Center => room / 2,
            ContentAlign.End => room,
            _ => 0
        };
    }
}
