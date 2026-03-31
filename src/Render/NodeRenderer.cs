using Limekuma.Render.Nodes;
using Limekuma.Render.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Limekuma.Render;

public static partial class NodeRenderer
{
    public static Image Render(CanvasNode root, AssetProvider assets, AssetProvider measurer)
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

    private static void RenderChildren(Image canvas, IEnumerable<Node> children, AssetProvider assets,
        AssetProvider measurer, Point origin, float inheritedOpacity, Size? desiredSize, float scale,
        ResamplerType resampler)
    {
        foreach (Node node in children)
        {
            RenderNode(canvas, node, assets, measurer, origin, inheritedOpacity, desiredSize, scale, resampler);
        }
    }

    private static void RenderNode(Image canvas, Node node, AssetProvider assets, AssetProvider measurer,
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

            case GridNode grid:
                RenderGridNode(canvas, grid, assets, measurer, origin, inheritedOpacity, desiredSize, scale, resampler);
                break;

            case ImageNode image:
                RenderImageNode(canvas, image, assets, origin, inheritedOpacity, desiredSize, scale, resampler);
                break;

            case TextNode text:
                RenderTextNode(canvas, text, assets, origin);
                break;

            case CanvasNode subCanvas:
                RenderCanvasNode(canvas, subCanvas, assets, measurer, origin, inheritedOpacity);
                break;
        }
    }
}