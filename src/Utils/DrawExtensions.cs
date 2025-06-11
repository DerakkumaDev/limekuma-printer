using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Limekuma.Utils;

internal static class DrawExtensions
{
    extension(FontFamily font)
    {
        internal Font GetSizeFont(float size) => new(font, size);

        internal Image DrawImage(float size, string text, Color color, IReadOnlyList<FontFamily> fallbacks,
            IResampler resampler, int zoom = 5)
        {
            float x = MathF.Log2(size);
            float y = x < zoom ? zoom : MathF.Ceiling(x);
            float z = MathF.Pow(2, y);

            FontRectangle textbox = font.GetSize(z, text, fallbacks);
            int textImageWidth = (int)Math.Ceiling(textbox.Width + z);
            int imageWidth = (int)Math.Ceiling(textImageWidth * size / z);
            int textImageHeight = (int)Math.Ceiling(textbox.Height + z);
            int imageHeight = (int)Math.Ceiling(textImageHeight * size / z);

            Font f = GetSizeFont(font, z);
            RichTextOptions options = new(f)
            {
                FallbackFontFamilies = fallbacks
            };

            Image<Rgba32> textImage = new(textImageWidth, textImageHeight);
            textImage.Mutate(ctx =>
            {
                ctx.DrawText(options, text, color);
                ctx.Resize(imageWidth, imageHeight, resampler);
            });

            return textImage;
        }

        internal Image DrawImage(float size, string text, Brush brush, Pen pen, IReadOnlyList<FontFamily> fallbacks,
            IResampler resampler, int zoom = 5)
        {
            float x = MathF.Log2(size);
            float y = x < zoom ? zoom : MathF.Ceiling(x);
            float z = MathF.Pow(2, y);

            FontRectangle textbox = font.GetSize(z, text, fallbacks);
            int textImageWidth = (int)Math.Ceiling(textbox.Width + z);
            int imageWidth = (int)Math.Ceiling(textImageWidth * size / z);
            int textImageHeight = (int)Math.Ceiling(textbox.Height + z);
            int imageHeight = (int)Math.Ceiling(textImageHeight * size / z);

            Font f = GetSizeFont(font, z);
            RichTextOptions options = new(f)
            {
                FallbackFontFamilies = fallbacks
            };

            Image<Rgba32> textImage = new(textImageWidth, textImageHeight);
            textImage.Mutate(ctx =>
            {
                ctx.DrawText(options, text, brush, pen);
                ctx.Resize(imageWidth, imageHeight, resampler);
            });

            return textImage;
        }

        internal FontRectangle GetSize(float size, string text, IReadOnlyList<FontFamily> fallbacks)
        {
            Font f = GetSizeFont(font, size);
            return TextMeasurer.MeasureAdvance(text, new(f)
            {
                FallbackFontFamilies = fallbacks
            });
        }
    }

    extension(Image image)
    {
        internal void Resize(int width, int height, IResampler resampler) =>
            image.Mutate(ctx => ctx.Resize(width, height, resampler));

        internal void Resize(double percent, IResampler resampler)
        {
            int imageWidth = (int)Math.Ceiling(image.Width * percent);
            int imageHeight = (int)Math.Ceiling(image.Height * percent);

            Resize(image, imageWidth, imageHeight, resampler);
        }
    }
}