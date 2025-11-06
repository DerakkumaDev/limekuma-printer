using Limekuma.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Limekuma.Render;

public interface IAssetProvider
{
    Image LoadImage(string resourceNamespace, string resourceKey);
    (FontFamily, List<FontFamily>) ResolveFont(string family);
}

public interface IMeasureService
{
    Size Measure(string text, string family, float size);
}

public static class Renderer
{
    public static Image Render(CanvasNode root, IAssetProvider assets, IMeasureService measurer)
    {
        Image<Rgba32> canvas = new(root.Width, root.Height);
        if (root.Background is { } bg)
        {
            canvas.Mutate(ctx => ctx.Fill(bg));
        }

        RenderChildren(canvas, root.Children, assets, measurer, new(0, 0), 1, null, 1, ResamplerType.Lanczos3);
        return canvas;
    }

    private static IResampler GetResampler(ResamplerType type) => type switch
    {
        ResamplerType.Bicubic => KnownResamplers.Bicubic,
        ResamplerType.Box => KnownResamplers.Box,
        ResamplerType.CatmullRom => KnownResamplers.CatmullRom,
        ResamplerType.Hermite => KnownResamplers.Hermite,
        ResamplerType.Lanczos2 => KnownResamplers.Lanczos2,
        ResamplerType.Lanczos3 => KnownResamplers.Lanczos3,
        ResamplerType.Lanczos5 => KnownResamplers.Lanczos5,
        ResamplerType.Lanczos8 => KnownResamplers.Lanczos8,
        ResamplerType.MitchellNetravali => KnownResamplers.MitchellNetravali,
        ResamplerType.NearestNeighbor => KnownResamplers.NearestNeighbor,
        ResamplerType.Robidoux => KnownResamplers.Robidoux,
        ResamplerType.RobidouxSharp => KnownResamplers.RobidouxSharp,
        ResamplerType.Spline => KnownResamplers.Spline,
        ResamplerType.Triangle => KnownResamplers.Triangle,
        ResamplerType.Welch => KnownResamplers.Welch,
        _ => KnownResamplers.Lanczos3
    };

    private static void RenderChildren(Image canvas, IEnumerable<Node> children, IAssetProvider assets,
        IMeasureService measurer, Point origin, float inheritedOpacity, Size? desiredSize, float scale,
        ResamplerType resampler)
    {
        foreach (Node node in children)
        {
            RenderNode(canvas, node, assets, measurer, origin, inheritedOpacity, desiredSize, scale, resampler);
        }
    }

    private static void RenderNode(Image canvas, Node node, IAssetProvider assets, IMeasureService measurer,
        Point origin, float inheritedOpacity, Size? desiredSize, float scale, ResamplerType resampler)
    {
        switch (node)
        {
            case LayerNode layer:
                RenderLayerNode(canvas, layer, assets, measurer, origin, inheritedOpacity, desiredSize, scale,
                    resampler);
                break;

            case PositionedNode pos:
                RenderPositionedNode(canvas, pos, assets, measurer, origin, inheritedOpacity, desiredSize, scale,
                    resampler);
                break;

            case ResizedNode resize:
                RenderResizedNode(canvas, resize, assets, measurer, origin, inheritedOpacity);
                break;

            case StackNode stack:
                RenderStackNode(canvas, stack, assets, measurer, origin, inheritedOpacity, desiredSize, scale,
                    resampler);
                break;

            case ImageNode image:
                RenderImageNode(canvas, image, assets, origin, inheritedOpacity, desiredSize, scale, resampler);
                break;

            case TextNode text:
                RenderTextNode(canvas, text, assets, origin);
                break;

            case CanvasNode subCanvas:
                RenderCanvasNode(canvas, subCanvas, assets, measurer, origin);
                break;
        }
    }

    private static void RenderLayerNode(Image canvas, LayerNode layer, IAssetProvider assets, IMeasureService measurer,
        Point origin, float inheritedOpacity, Size? desiredSize, float scale, ResamplerType resampler) =>
        RenderChildren(canvas, layer.Children, assets, measurer, origin, inheritedOpacity * layer.Opacity, desiredSize,
            scale, resampler);

    private static void RenderPositionedNode(Image canvas, PositionedNode pos, IAssetProvider assets,
        IMeasureService measurer, Point origin, float inheritedOpacity, Size? desiredSize, float scale,
        ResamplerType resampler) => RenderNode(canvas, pos.Child, assets, measurer,
        new(origin.X + pos.Position.X, origin.Y + pos.Position.Y), inheritedOpacity, desiredSize, scale, resampler);

    private static void RenderResizedNode(Image canvas, ResizedNode resize, IAssetProvider assets,
        IMeasureService measurer, Point origin, float inheritedOpacity) => RenderNode(canvas, resize.Child, assets,
        measurer, origin, inheritedOpacity, resize.DesiredSize, resize.Scale, resize.Resampler);

    private static void RenderStackNode(Image canvas, StackNode stack, IAssetProvider assets, IMeasureService measurer,
        Point origin, float inheritedOpacity, Size? desiredSize, float scale, ResamplerType resampler)
    {
        Point offset = origin;
        for (int i = 0; i < stack.Children.Count; ++i)
        {
            Node child = stack.Children[i];
            RenderNode(canvas, child, assets, measurer, offset, inheritedOpacity, desiredSize, scale, resampler);
            Size sz = Measure(child, assets, measurer);
            offset = stack.Direction is StackDirection.Row
                ? new(offset.X + sz.Width + (i > 0 ? stack.Spacing : 0), offset.Y)
                : new(offset.X, offset.Y + sz.Height + (i > 0 ? stack.Spacing : 0));
        }
    }

    private static void RenderImageNode(Image canvas, ImageNode image, IAssetProvider assets, Point origin,
        float inheritedOpacity, Size? desiredSize, float scale, ResamplerType resampler)
    {
        Image img = assets.LoadImage(image.Namespace, image.ResourceKey);
        IResampler resamplerInstance = GetResampler(resampler);
        if (desiredSize is { } desired)
        {
            img.Mutate(c => c.Resize(desired.Width, desired.Height, resamplerInstance));
        }

        if (Math.Abs(scale - 1) > 0.0000001)
        {
            img.Mutate(c => c.Resize((int)Math.Round(img.Width * scale), (int)Math.Round(img.Height * scale),
                resamplerInstance));
        }

        canvas.Mutate(c => c.DrawImage(img, origin, inheritedOpacity));
    }

    private static void RenderTextNode(Image canvas, TextNode text, IAssetProvider assets, PointF origin)
    {
        (FontFamily mainFont, List<FontFamily> fallbacks) = assets.ResolveFont(text.FontFamily);
        Font font = new(mainFont, text.FontSize * (115 / 18));
        RichTextOptions options = new(font)
        {
            Font = font,
            FallbackFontFamilies = fallbacks,
            Origin = origin,
            TextAlignment = text.TextAlignment,
            VerticalAlignment = text.VerticalAlignment,
            HorizontalAlignment = text.HorizontalAlignment,
            HintingMode = HintingMode.Standard,
            Dpi = 460
        };
        canvas.Mutate(ctx => ctx.DrawText(options, text.Text, Brushes.Solid(text.Color),
            text is { StrokeColor: { } sc, StrokeWidth: { } sw } ? Pens.Solid(sc, sw) : null));
    }

    private static void RenderCanvasNode(Image canvasImage, CanvasNode canvas, IAssetProvider assets,
        IMeasureService measurer,
        Point origin)
    {
        using Image subCanvas = Render(canvas, assets, measurer);
        canvasImage.Mutate(c => c.DrawImage(subCanvas, origin, 1));
    }

    private static Size Measure(Node node, IAssetProvider assets, IMeasureService measurer) => node switch
    {
        ImageNode image => MeasureImageNode(image, assets),
        TextNode text => MeasureTextNode(text, measurer),
        StackNode stack => MeasureStackNode(stack, assets, measurer),
        LayerNode layer => MeasureLayerNode(layer, assets, measurer),
        PositionedNode pos => Measure(pos.Child, assets, measurer),
        _ => Size.Empty
    };

    private static Size MeasureImageNode(ImageNode image, IAssetProvider assets)
    {
        Image img = assets.LoadImage(image.Namespace, image.ResourceKey);
        return new(img.Width, img.Height);
    }

    private static Size MeasureTextNode(TextNode text, IMeasureService measurer) =>
        measurer.Measure(text.Text, text.FontFamily, text.FontSize);

    private static Size MeasureStackNode(StackNode stack, IAssetProvider assets, IMeasureService measurer)
    {
        int width = 0, height = 0;
        for (int i = 0; i < stack.Children.Count; ++i)
        {
            Size child = Measure(stack.Children[i], assets, measurer);
            if (stack.Direction is StackDirection.Row)
            {
                width += child.Width + (i > 0 ? stack.Spacing : 0);
                height = Math.Max(height, child.Height);
            }
            else
            {
                height += child.Height + (i > 0 ? stack.Spacing : 0);
                width = Math.Max(width, child.Width);
            }
        }

        return new(width, height);
    }

    private static Size MeasureLayerNode(LayerNode layer, IAssetProvider assets, IMeasureService measurer)
    {
        int width = 0, height = 0;
        foreach (Node child in layer.Children)
        {
            Size size = Measure(child, assets, measurer);
            width = Math.Max(width, size.Width);
            height = Math.Max(height, size.Height);
        }

        return new(width, height);
    }
}