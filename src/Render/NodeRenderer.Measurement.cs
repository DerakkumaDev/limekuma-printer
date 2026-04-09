using Limekuma.Render.Nodes;
using Limekuma.Render.Types;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;

namespace Limekuma.Render;

public static partial class NodeRenderer
{
    private static Size Measure(Node node, AssetProvider assets, AssetProvider measurer,
        Dictionary<Node, Size> measurementCache)
    {
        if (measurementCache.TryGetValue(node, out Size cached))
        {
            return cached;
        }

        Size measured = node switch
        {
            ImageNode image => MeasureImageNode(image, assets),
            TextNode text => MeasureTextNode(text, measurer),
            ResizedNode resized => MeasureResizedNode(resized, assets, measurer, measurementCache),
            StackNode stack => MeasureStackNode(stack, assets, measurer, measurementCache),
            GridNode grid => MeasureGridNode(grid, assets, measurer, measurementCache),
            LayerNode layer => MeasureLayerNode(layer, assets, measurer, measurementCache),
            PositionedNode pos => MeasurePositionedNode(pos, assets, measurer, measurementCache),
            CanvasNode canvas => new(canvas.Width, canvas.Height),
            _ => Size.Empty
        };
        measurementCache[node] = measured;
        return measured;
    }

    private static Size MeasureImageNode(ImageNode image, AssetProvider assets)
    {
        Image img = assets.LoadImage(image.Namespace, image.ResourceKey);
        return new(img.Width, img.Height);
    }

    private static Size MeasureTextNode(TextNode text, AssetProvider measurer)
    {
        (FontFamily mainFont, List<FontFamily> fallbacks) = measurer.ResolveFont(text.FontFamily);
        float scaledFontSize = text.FontSize * 72 / 300;
        Font font = new(mainFont, scaledFontSize);
        RichTextOptions options = new(font)
        {
            Font = font,
            FallbackFontFamilies = fallbacks,
            TextAlignment = text.TextAlignment,
            VerticalAlignment = text.VerticalAlignment,
            HorizontalAlignment = text.HorizontalAlignment,
            HintingMode = HintingMode.Standard,
            Dpi = 300
        };

        string measuredText = text.Text;
        if (text is { TruncateWidth: { } tw, TruncateSuffix: { } ts })
        {
            measuredText = TruncateTextByWidth(measuredText, ts, tw, options);
        }

        FontRectangle rect = TextMeasurer.MeasureAdvance(measuredText, options);
        return new((int)Math.Ceiling(rect.Width), (int)Math.Ceiling(rect.Height));
    }

    private static Size MeasureResizedNode(ResizedNode resized, AssetProvider assets, AssetProvider measurer,
        Dictionary<Node, Size> measurementCache)
    {
        Size baseSize = Measure(resized.Child, assets, measurer, measurementCache);
        int width = resized.Width ?? baseSize.Width;
        int height = resized.Height ?? baseSize.Height;
        width = Math.Max(1, (int)Math.Round(width * resized.Scale));
        height = Math.Max(1, (int)Math.Round(height * resized.Scale));
        return new(width, height);
    }

    private static Size MeasureStackNode(StackNode stack, AssetProvider assets, AssetProvider measurer,
        Dictionary<Node, Size> measurementCache)
    {
        List<Node> flowChildren = ExpandFlowChildren(stack.Children);
        List<(Node Node, Size Size)> items =
            [.. flowChildren.Select(c => (c, Measure(c, assets, measurer, measurementCache)))];
        bool isRow = stack.Direction is StackDirection.Row;
        float wrapMain = ResolveMainAxisContainerSize(isRow ? stack.Width : stack.Height, null, int.MaxValue);
        List<List<(Node Node, Size Size)>> lines = ResolveStackLines(items, isRow, stack.Wrap, stack.Spacing, wrapMain);
        List<(float Main, int Cross)> lineSizes = ResolveStackLineSizes(lines, isRow, stack.Spacing);
        (float contentMain, float contentCross) = ResolveStackContentSize(lineSizes, stack.RunSpacing);

        int contentWidth = (int)Math.Ceiling(isRow ? contentMain : contentCross);
        int contentHeight = (int)Math.Ceiling(isRow ? contentCross : contentMain);
        return new(stack.Width ?? contentWidth, stack.Height ?? contentHeight);
    }

    private static Size MeasureGridNode(GridNode grid, AssetProvider assets, AssetProvider measurer,
        Dictionary<Node, Size> measurementCache)
    {
        List<Node> flowChildren = ExpandFlowChildren(grid.Children);
        if (flowChildren.Count is 0)
        {
            return Size.Empty;
        }

        int columns = Math.Max(1, grid.Columns);
        int rows = (int)Math.Ceiling(flowChildren.Count / (double)columns);
        int[] colWidths = new int[columns];
        int[] rowHeights = new int[rows];
        for (int i = 0; i < flowChildren.Count; i++)
        {
            Size size = Measure(flowChildren[i], assets, measurer, measurementCache);
            int r = i / columns;
            int c = i % columns;
            colWidths[c] = Math.Max(colWidths[c], size.Width);
            rowHeights[r] = Math.Max(rowHeights[r], size.Height);
        }

        int width = colWidths.Sum() + Math.Max(0, columns - 1) * grid.ColumnGap;
        int height = rowHeights.Sum() + Math.Max(0, rows - 1) * grid.RowGap;
        return new(grid.Width ?? width, grid.Height ?? height);
    }

    private static Size MeasurePositionedNode(PositionedNode pos, AssetProvider assets, AssetProvider measurer,
        Dictionary<Node, Size> measurementCache)
    {
        Size contentSize = MeasureChildren(pos.Children, assets, measurer, measurementCache);
        int width = Math.Max(pos.Width ?? 0, contentSize.Width);
        int height = Math.Max(pos.Height ?? 0, contentSize.Height);
        return new(pos.Position.X + width, pos.Position.Y + height);
    }

    private static Size MeasureLayerNode(LayerNode layer, AssetProvider assets, AssetProvider measurer,
        Dictionary<Node, Size> measurementCache) => MeasureChildren(layer.Children, assets, measurer, measurementCache);

    private static Size MeasureChildren(IEnumerable<Node> children, AssetProvider assets, AssetProvider measurer,
        Dictionary<Node, Size> measurementCache)
    {
        int width = 0;
        int height = 0;
        foreach (Node child in children)
        {
            Size size = Measure(child, assets, measurer, measurementCache);
            width = Math.Max(width, size.Width);
            height = Math.Max(height, size.Height);
        }

        return new(width, height);
    }

    private static List<Node> ExpandFlowChildren(IEnumerable<Node> children)
    {
        List<Node> output = [];
        foreach (Node child in children)
        {
            if (child is LayerNode { Opacity: 1, Key: null, Children: var nested })
            {
                output.AddRange(ExpandFlowChildren(nested));
                continue;
            }

            output.Add(child);
        }

        return output;
    }
}
