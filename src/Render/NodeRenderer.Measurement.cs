using Limekuma.Render.Nodes;
using Limekuma.Render.Types;
using SixLabors.ImageSharp;

namespace Limekuma.Render;

public static partial class NodeRenderer
{
    private static Size Measure(Node node, AssetProvider assets, AssetProvider measurer) => node switch
    {
        ImageNode image => MeasureImageNode(image, assets),
        TextNode text => MeasureTextNode(text, measurer),
        StackNode stack => MeasureStackNode(stack, assets, measurer),
        GridNode grid => MeasureGridNode(grid, assets, measurer),
        LayerNode layer => MeasureLayerNode(layer, assets, measurer),
        PositionedNode pos => MeasurePositionedNode(pos, assets, measurer),
        CanvasNode canvas => new(canvas.Width, canvas.Height),
        _ => Size.Empty
    };

    private static Size MeasureImageNode(ImageNode image, AssetProvider assets)
    {
        using Image img = assets.LoadImage(image.Namespace, image.ResourceKey);
        return new(img.Width, img.Height);
    }

    private static Size MeasureTextNode(TextNode text, AssetProvider measurer) =>
        measurer.Measure(text.Text, text.FontFamily, text.FontSize);

    private static Size MeasureStackNode(StackNode stack, AssetProvider assets, AssetProvider measurer)
    {
        List<Node> flowChildren = ExpandFlowChildren(stack.Children);
        int width = 0, height = 0;
        for (int i = 0; i < flowChildren.Count; ++i)
        {
            Size child = Measure(flowChildren[i], assets, measurer);
            if (stack.Direction is StackDirection.Row)
            {
                width += child.Width + (i < flowChildren.Count - 1 ? stack.Spacing : 0);
                height = Math.Max(height, child.Height);
            }
            else
            {
                height += child.Height + (i < flowChildren.Count - 1 ? stack.Spacing : 0);
                width = Math.Max(width, child.Width);
            }
        }

        return new(width, height);
    }

    private static Size MeasureGridNode(GridNode grid, AssetProvider assets, AssetProvider measurer)
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
            Size size = Measure(flowChildren[i], assets, measurer);
            int r = i / columns;
            int c = i % columns;
            colWidths[c] = Math.Max(colWidths[c], size.Width);
            rowHeights[r] = Math.Max(rowHeights[r], size.Height);
        }

        int width = colWidths.Sum() + Math.Max(0, columns - 1) * grid.ColumnGap;
        int height = rowHeights.Sum() + Math.Max(0, rows - 1) * grid.RowGap;
        return new(grid.Width ?? width, grid.Height ?? height);
    }

    private static Size MeasurePositionedNode(PositionedNode pos, AssetProvider assets, AssetProvider measurer)
    {
        Size contentSize = MeasureChildren(pos.Children, assets, measurer);
        int width = Math.Max(pos.Width ?? 0, contentSize.Width);
        int height = Math.Max(pos.Height ?? 0, contentSize.Height);
        return new(pos.Position.X + width, pos.Position.Y + height);
    }

    private static Size MeasureLayerNode(LayerNode layer, AssetProvider assets, AssetProvider measurer) =>
        MeasureChildren(layer.Children, assets, measurer);

    private static Size MeasureChildren(IEnumerable<Node> children, AssetProvider assets, AssetProvider measurer)
    {
        int width = 0;
        int height = 0;
        foreach (Node child in children)
        {
            Size size = Measure(child, assets, measurer);
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