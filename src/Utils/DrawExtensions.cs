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
            IResampler resampler)
        {
            float x = MathF.Log2(size);
            // 2^5 = 32
            float y = x < 5 ? 5 : MathF.Ceiling(x);
            float z = MathF.Pow(2, y);

            Font f = new(font, z);
            FontRectangle textbox = TextMeasurer.MeasureAdvance(text, new(f)
            {
                FallbackFontFamilies = fallbacks
            });

            Image textImage = new Image<Rgba32>((int)Math.Ceiling(textbox.Width + z),
                (int)Math.Ceiling(textbox.Height + z));
            textImage.Mutate(ctx =>
            {
                ctx.DrawText(new(f)
                {
                    FallbackFontFamilies = fallbacks
                }, text, color);
                ctx.Resize((int)Math.Ceiling(textImage.Width * size / z),
                    (int)Math.Ceiling(textImage.Height * size / z), resampler);
            });

            return textImage;
        }

        internal Image DrawImage(float size, string text, Brush brush, Pen pen, IReadOnlyList<FontFamily> fallbacks,
            IResampler resampler)
        {
            float x = MathF.Log2(size);
            // 2^5 = 32
            float y = x < 5 ? 5 : MathF.Ceiling(x);
            float z = MathF.Pow(2, y);

            Font f = new(font, z);
            FontRectangle textbox = TextMeasurer.MeasureAdvance(text, new(f)
            {
                FallbackFontFamilies = fallbacks
            });

            Image textImage = new Image<Rgba32>((int)Math.Ceiling(textbox.Width + z),
                (int)Math.Ceiling(textbox.Height + z));
            textImage.Mutate(ctx =>
            {
                ctx.DrawText(new(f)
                {
                    FallbackFontFamilies = fallbacks
                }, text, brush, pen);
                ctx.Resize((int)Math.Ceiling(textImage.Width * size / z),
                    (int)Math.Ceiling(textImage.Height * size / z), resampler);
            });

            return textImage;
        }
    }

    extension(Image image)
    {
        internal void Resize(int width, int height, IResampler resampler) =>
            image.Mutate(ctx => ctx.Resize(width, height, resampler));

        internal void Resize(double percent, IResampler resampler) =>
            image.Mutate(ctx => ctx.Resize((int)Math.Ceiling(image.Width * percent),
                (int)Math.Ceiling(image.Height * percent), resampler));
    }
}