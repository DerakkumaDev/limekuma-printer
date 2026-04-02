using Limekuma.Render.Nodes;
using Limekuma.Render.Types;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Globalization;

namespace Limekuma.Render;

public static partial class NodeRenderer
{
    private static void RenderLayerNode(Image canvas, LayerNode layer, AssetProvider assets, AssetProvider measurer,
        Point origin, float inheritedOpacity, Size? desiredSize, float scale, ResamplerType resampler,
        Dictionary<Node, Size> measurementCache) =>
        RenderChildren(canvas, layer.Children, assets, measurer, origin, inheritedOpacity * layer.Opacity, desiredSize,
            scale, resampler, measurementCache);

    private static void RenderPositionedNode(Image canvas, PositionedNode pos, AssetProvider assets,
        AssetProvider measurer, Point origin, float inheritedOpacity, Size? desiredSize, float scale,
        ResamplerType resampler, Dictionary<Node, Size> measurementCache)
    {
        Size contentSize = MeasureChildren(pos.Children, assets, measurer, measurementCache);
        int containerWidth = pos.Width ?? contentSize.Width;
        int containerHeight = pos.Height ?? contentSize.Height;
        int offsetX = pos.AnchorX switch
        {
            PositionAnchor.Center => containerWidth / 2,
            PositionAnchor.End => containerWidth,
            _ => 0
        };
        int offsetY = pos.AnchorY switch
        {
            PositionAnchor.Center => containerHeight / 2,
            PositionAnchor.End => containerHeight,
            _ => 0
        };

        Point childOrigin = new(origin.X + pos.Position.X - offsetX, origin.Y + pos.Position.Y - offsetY);
        Size? childDesiredSize = (pos.Width.HasValue || pos.Height.HasValue)
            ? new(containerWidth, containerHeight)
            : desiredSize;
        RenderChildren(canvas, pos.Children, assets, measurer, childOrigin, inheritedOpacity, childDesiredSize, scale,
            resampler, measurementCache);
    }

    private static void RenderResizedNode(Image canvas, ResizedNode resize, AssetProvider assets,
        AssetProvider measurer, Point origin, float inheritedOpacity, Dictionary<Node, Size> measurementCache)
    {
        Size naturalSize = Measure(resize.Child, assets, measurer, measurementCache);
        int baseWidth = Math.Max(1, naturalSize.Width);
        int baseHeight = Math.Max(1, naturalSize.Height);

        using Image<Rgba32> rendered = new(baseWidth, baseHeight);
        RenderNode(rendered, resize.Child, assets, measurer, new(0, 0), 1, null, 1, resize.Resampler,
            measurementCache);

        int width = resize.Width ?? rendered.Width;
        int height = resize.Height ?? rendered.Height;
        width = Math.Max(1, (int)Math.Round(width * resize.Scale));
        height = Math.Max(1, (int)Math.Round(height * resize.Scale));
        rendered.Mutate(ctx => ctx.Resize(width, height, GetResampler(resize.Resampler)));
        bool shouldInheritMetadata = canvas.Frames.Count <= 1 && rendered.Frames.Count > 1;
        canvas.Mutate(ctx => ctx.DrawImage(rendered, origin, inheritedOpacity, GetForegroundRepeatCount(rendered)));
        if (shouldInheritMetadata)
        {
            InheritCanvasFramesAndMetadataOnFirstDraw(canvas, rendered);
        }
    }

    private static void RenderStackNode(Image canvas, StackNode stack, AssetProvider assets, AssetProvider measurer,
        Point origin, float inheritedOpacity, Size? desiredSize, float scale, ResamplerType resampler,
        Dictionary<Node, Size> measurementCache)
    {
        List<Node> flowChildren = ExpandFlowChildren(stack.Children);
        List<(Node Node, Size Size)> items =
        [
            .. flowChildren.Select(c => (c, Measure(c, assets, measurer, measurementCache)))
        ];
        if (items.Count is 0)
        {
            return;
        }

        bool isRow = stack.Direction is StackDirection.Row;
        float wrapMain = ResolveMainAxisContainerSize(
            isRow ? stack.Width : stack.Height,
            null,
            int.MaxValue);
        List<List<(Node Node, Size Size)>> lines =
            ResolveStackLines(items, isRow, stack.Wrap, stack.Spacing, wrapMain);
        List<(float Main, int Cross)> lineSizes = ResolveStackLineSizes(lines, isRow, stack.Spacing);
        (float contentMain, float contentCross) = ResolveStackContentSize(lineSizes, stack.RunSpacing);
        float resolvedContainerMain = ResolveMainAxisContainerSize(
            isRow ? stack.Width : stack.Height,
            isRow ? desiredSize?.Width : desiredSize?.Height,
            contentMain);
        float resolvedContainerCross = ResolveMainAxisContainerSize(
            isRow ? stack.Height : stack.Width,
            isRow ? desiredSize?.Height : desiredSize?.Width,
            contentCross);

        float crossBase = (isRow ? origin.Y : origin.X) +
            ResolveStartCenterEndOffset(stack.AlignContent, resolvedContainerCross, contentCross);
        int crossCursor = (int)Math.Round(crossBase, MidpointRounding.AwayFromZero);
        float runError = crossBase - crossCursor;
        int runSpacingWhole = stack.RunSpacing >= 0
            ? (int)Math.Floor(stack.RunSpacing)
            : (int)Math.Ceiling(stack.RunSpacing);
        float runSpacingFraction = stack.RunSpacing - runSpacingWhole;
        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            List<(Node Node, Size Size)> line = lines[lineIndex];
            (float lineMain, int lineCross) = lineSizes[lineIndex];
            (float startMain, float between) = ResolveMainAxisLayout(stack.JustifyContent, resolvedContainerMain, lineMain,
                stack.Spacing, line.Count);
            float mainBase = (isRow ? origin.X : origin.Y) + startMain;
            int mainCursor = (int)Math.Round(mainBase, MidpointRounding.AwayFromZero);
            float gapError = mainBase - mainCursor;
            int betweenWhole = between >= 0
                ? (int)Math.Floor(between)
                : (int)Math.Ceiling(between);
            float betweenFraction = between - betweenWhole;
            for (int itemIndex = 0; itemIndex < line.Count; itemIndex++)
            {
                (Node node, Size size) = line[itemIndex];
                int itemCross = isRow ? size.Height : size.Width;
                int crossOffset = ResolveStartCenterEndOffset(stack.AlignItems, lineCross, itemCross);
                Size? childDesiredSize = null;
                if (stack.AlignItems is AlignItems.Stretch)
                {
                    childDesiredSize = isRow ? new(size.Width, lineCross) : new(lineCross, size.Height);
                }

                float childX = isRow ? mainCursor : crossCursor + crossOffset;
                float childY = isRow ? crossCursor + crossOffset : mainCursor;
                Point childOrigin = new(
                    (int)Math.Round(childX, MidpointRounding.AwayFromZero),
                    (int)Math.Round(childY, MidpointRounding.AwayFromZero));
                RenderNode(canvas, node, assets, measurer, childOrigin, inheritedOpacity, childDesiredSize, scale,
                    resampler, measurementCache);
                if (itemIndex == line.Count - 1)
                {
                    continue;
                }

                int itemMain = isRow ? size.Width : size.Height;
                int step = itemMain + betweenWhole;
                gapError += betweenFraction;
                if (gapError >= 1f)
                {
                    step++;
                    gapError -= 1f;
                }
                else if (gapError <= -1f)
                {
                    step--;
                    gapError += 1f;
                }

                mainCursor += step;
            }

            if (lineIndex == lines.Count - 1)
            {
                continue;
            }

            int lineStep = lineCross + runSpacingWhole;
            runError += runSpacingFraction;
            if (runError >= 1f)
            {
                lineStep++;
                runError -= 1f;
            }
            else if (runError <= -1f)
            {
                lineStep--;
                runError += 1f;
            }

            crossCursor += lineStep;
        }
    }

    private static void RenderGridNode(Image canvas, GridNode grid, AssetProvider assets, AssetProvider measurer,
        Point origin, float inheritedOpacity, Size? desiredSize, float scale, ResamplerType resampler,
        Dictionary<Node, Size> measurementCache)
    {
        List<Node> flowChildren = ExpandFlowChildren(grid.Children);
        if (flowChildren.Count == 0)
        {
            return;
        }

        int columns = Math.Max(1, grid.Columns);
        int rows = (int)Math.Ceiling(flowChildren.Count / (double)columns);
        Size[] sizes = [.. flowChildren.Select(c => Measure(c, assets, measurer, measurementCache))];
        int[] colWidths = new int[columns];
        int[] rowHeights = new int[rows];
        for (int i = 0; i < flowChildren.Count; i++)
        {
            int r = i / columns;
            int c = i % columns;
            colWidths[c] = Math.Max(colWidths[c], sizes[i].Width);
            rowHeights[r] = Math.Max(rowHeights[r], sizes[i].Height);
        }

        int[] colX = new int[columns];
        int[] rowY = new int[rows];
        for (int c = 1; c < columns; c++)
        {
            colX[c] = colX[c - 1] + colWidths[c - 1] + grid.ColumnGap;
        }

        for (int r = 1; r < rows; r++)
        {
            rowY[r] = rowY[r - 1] + rowHeights[r - 1] + grid.RowGap;
        }

        int contentWidth = colWidths.Sum() + Math.Max(0, columns - 1) * grid.ColumnGap;
        int contentHeight = rowHeights.Sum() + Math.Max(0, rows - 1) * grid.RowGap;
        int containerWidth = grid.Width ?? desiredSize?.Width ?? contentWidth;
        int containerHeight = grid.Height ?? desiredSize?.Height ?? contentHeight;
        int gridOffsetX = ResolveStartCenterEndOffset(grid.JustifyContent, containerWidth, contentWidth);
        int gridOffsetY = ResolveStartCenterEndOffset(grid.AlignContent, containerHeight, contentHeight);

        for (int i = 0; i < flowChildren.Count; i++)
        {
            int r = i / columns;
            int c = i % columns;
            Size size = sizes[i];
            int cellWidth = colWidths[c];
            int cellHeight = rowHeights[r];
            int childXOffset = ResolveStartCenterEndOffset(grid.JustifyItems, cellWidth, size.Width);
            int childYOffset = ResolveStartCenterEndOffset(grid.AlignItems, cellHeight, size.Height);
            Size? childDesiredSize = null;
            if (grid.JustifyItems is AlignItems.Stretch || grid.AlignItems is AlignItems.Stretch)
            {
                childDesiredSize = new(
                    grid.JustifyItems is AlignItems.Stretch ? cellWidth : size.Width,
                    grid.AlignItems is AlignItems.Stretch ? cellHeight : size.Height);
            }

            Point childOrigin = new(origin.X + gridOffsetX + colX[c] + childXOffset,
                origin.Y + gridOffsetY + rowY[r] + childYOffset);
            RenderNode(canvas, flowChildren[i], assets, measurer, childOrigin, inheritedOpacity, childDesiredSize, scale,
                resampler, measurementCache);
        }
    }

    private static void RenderImageNode(Image canvas, ImageNode image, AssetProvider assets, Point origin,
        float inheritedOpacity, Size? desiredSize, float scale, ResamplerType resampler)
    {
        using Image img = assets.LoadImage(image.Namespace, image.ResourceKey).CloneAs<Rgba32>();
        IResampler resamplerInstance = GetResampler(resampler);
        if (desiredSize is { } desired && desired.Width > 0 && desired.Height > 0)
        {
            img.Mutate(c => c.Resize(desired.Width, desired.Height, resamplerInstance));
        }

        if (Math.Abs(scale - 1) > 0.0000001)
        {
            int width = Math.Max(1, (int)Math.Round(img.Width * scale));
            int height = Math.Max(1, (int)Math.Round(img.Height * scale));
            img.Mutate(c => c.Resize(width, height, resamplerInstance));
        }

        bool shouldInheritMetadata = canvas.Frames.Count <= 1 && img.Frames.Count > 1;
        canvas.Mutate(c => c.DrawImage(img, origin, image.ColorBlending, image.AlphaComposition, inheritedOpacity,
            image.ForegroundRepeatCount));
        if (shouldInheritMetadata)
        {
            InheritCanvasFramesAndMetadataOnFirstDraw(canvas, img);
        }
    }

    private static void RenderTextNode(Image canvas, TextNode textNode, AssetProvider assets, PointF origin,
        float inheritedOpacity)
    {
        (FontFamily mainFont, List<FontFamily> fallbacks) = assets.ResolveFont(textNode.FontFamily);
        float scaledFontSize = textNode.FontSize * 72 / 300;
        Font font = new(mainFont, scaledFontSize);
        RichTextOptions options = new(font)
        {
            Font = font,
            FallbackFontFamilies = fallbacks,
            Origin = origin,
            TextAlignment = textNode.TextAlignment,
            VerticalAlignment = textNode.VerticalAlignment,
            HorizontalAlignment = textNode.HorizontalAlignment,
            HintingMode = HintingMode.Standard,
            Dpi = 300
        };

        string drawText = textNode.Text;
        if (textNode is { TruncateWidth: { } tw, TruncateSuffix: { } ts })
        {
            drawText = TruncateTextByWidth(drawText, ts, tw, options);
        }

        Color fillColor = ApplyOpacity(textNode.Color, inheritedOpacity);
        if (textNode is { StrokeColor: { } sc, StrokeWidth: > 0 and var sw })
        {
            Color strokeColor = ApplyOpacity(sc, inheritedOpacity);
            canvas.Mutate(ctx => ctx.DrawText(options, drawText, Brushes.Solid(fillColor), Pens.Solid(strokeColor, sw)));
            return;
        }

        canvas.Mutate(ctx => ctx.DrawText(options, drawText, fillColor));
    }

    private static Color ApplyOpacity(Color color, float opacity)
    {
        float clampedOpacity = Math.Clamp(opacity, 0f, 1f);
        Rgba32 rgba = color.ToPixel<Rgba32>();
        byte alpha = (byte)Math.Round(rgba.A * clampedOpacity);
        return Color.FromPixel(new Rgba32(rgba.R, rgba.G, rgba.B, alpha));
    }

    private static string TruncateTextByWidth(string text, string suffix, float maxWidth, RichTextOptions options)
    {
        if (maxWidth <= 0)
        {
            return string.Empty;
        }

        if (TextMeasurer.MeasureAdvance(text, options).Width <= maxWidth)
        {
            return text;
        }

        if (TextMeasurer.MeasureAdvance(suffix, options).Width > maxWidth)
        {
            return string.Empty;
        }

        int[] textElementStarts = StringInfo.ParseCombiningCharacters(text);
        int low = 0;
        int high = textElementStarts.Length;
        while (low < high)
        {
            int mid = (low + high + 1) / 2;
            string candidate = GetTextElementPrefix(text, textElementStarts, mid) + suffix;
            if (TextMeasurer.MeasureAdvance(candidate, options).Width <= maxWidth)
            {
                low = mid;
                continue;
            }

            high = mid - 1;
        }

        return GetTextElementPrefix(text, textElementStarts, low) + suffix;
    }

    private static string GetTextElementPrefix(string text, int[] textElementStarts, int count)
    {
        if (count <= 0 || textElementStarts.Length == 0)
        {
            return string.Empty;
        }

        int end = count >= textElementStarts.Length ? text.Length : textElementStarts[count];
        return text[..end];
    }

    private static void RenderCanvasNode(Image canvasImage, CanvasNode canvas, AssetProvider assets,
        AssetProvider measurer, Point origin, float inheritedOpacity)
    {
        using Image subCanvas = Render(canvas, assets, measurer);
        bool shouldInheritMetadata = canvasImage.Frames.Count <= 1 && subCanvas.Frames.Count > 1;
        canvasImage.Mutate(c => c.DrawImage(subCanvas, origin, inheritedOpacity, GetForegroundRepeatCount(subCanvas)));
        if (shouldInheritMetadata)
        {
            InheritCanvasFramesAndMetadataOnFirstDraw(canvasImage, subCanvas);
        }
    }

    private static void InheritCanvasFramesAndMetadataOnFirstDraw(Image canvas, Image foreground)
    {
        if (foreground.Frames.Count <= 1)
        {
            return;
        }

        int diff = foreground.Frames.Count - canvas.Frames.Count;
        if (diff > 0)
        {
            for (int i = 0; i < diff; ++i)
            {
                canvas.Frames.AddFrame(canvas.Frames[i]);
            }
        }

        GifMetadata sourceGifMetadata = foreground.Metadata.GetGifMetadata();
        GifMetadata targetGifMetadata = canvas.Metadata.GetGifMetadata();
        targetGifMetadata.RepeatCount = sourceGifMetadata.RepeatCount;

        PngMetadata sourcePngMetadata = foreground.Metadata.GetPngMetadata();
        PngMetadata targetPngMetadata = canvas.Metadata.GetPngMetadata();
        targetPngMetadata.RepeatCount = sourcePngMetadata.RepeatCount;

        WebpMetadata sourceWebpMetadata = foreground.Metadata.GetWebpMetadata();
        WebpMetadata targetWebpMetadata = canvas.Metadata.GetWebpMetadata();
        targetWebpMetadata.RepeatCount = sourceWebpMetadata.RepeatCount;

        for (int i = 0; i < foreground.Frames.Count; i++)
        {
            GifFrameMetadata sourceFrameMetadata = foreground.Frames[i].Metadata.GetGifMetadata();
            GifFrameMetadata targetFrameMetadata = canvas.Frames[i].Metadata.GetGifMetadata();
            targetFrameMetadata.FrameDelay = sourceFrameMetadata.FrameDelay;

            PngFrameMetadata sourcePngFrameMetadata = foreground.Frames[i].Metadata.GetPngMetadata();
            PngFrameMetadata targetPngFrameMetadata = canvas.Frames[i].Metadata.GetPngMetadata();
            targetPngFrameMetadata.FrameDelay = sourcePngFrameMetadata.FrameDelay;
            targetPngFrameMetadata.DisposalMode = sourcePngFrameMetadata.DisposalMode;
            targetPngFrameMetadata.BlendMode = sourcePngFrameMetadata.BlendMode;

            WebpFrameMetadata sourceWebpFrameMetadata = foreground.Frames[i].Metadata.GetWebpMetadata();
            WebpFrameMetadata targetWebpFrameMetadata = canvas.Frames[i].Metadata.GetWebpMetadata();
            targetWebpFrameMetadata.FrameDelay = sourceWebpFrameMetadata.FrameDelay;
            targetWebpFrameMetadata.DisposalMode = sourceWebpFrameMetadata.DisposalMode;
            targetWebpFrameMetadata.BlendMode = sourceWebpFrameMetadata.BlendMode;
        }
    }

    private static int GetForegroundRepeatCount(Image foreground)
    {
        string? formatName = foreground.Metadata.DecodedImageFormat?.Name;
        if (string.Equals(formatName, "GIF", StringComparison.OrdinalIgnoreCase))
        {
            return foreground.Metadata.GetGifMetadata().RepeatCount;
        }

        if (string.Equals(formatName, "PNG", StringComparison.OrdinalIgnoreCase))
        {
            return (int)Math.Min(foreground.Metadata.GetPngMetadata().RepeatCount, int.MaxValue);
        }

        if (string.Equals(formatName, "WEBP", StringComparison.OrdinalIgnoreCase))
        {
            return (int)Math.Min(foreground.Metadata.GetWebpMetadata().RepeatCount, int.MaxValue);
        }

        return 0;
    }
}