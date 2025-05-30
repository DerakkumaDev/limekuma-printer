using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace DXKumaBot.Backend.Utils;

internal static class DrawExtensions
{
    extension(FontFamily font)
    {
        public Font GetSizeFont(float size) => new(font, size);

        public Image DrawImage(float size, string text, Color color, IReadOnlyList<FontFamily> fallbacks, IResampler resampler)
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

            Image textImage = new Image<Rgba32>(Convert.ToInt32(Math.Ceiling(textbox.Width + z)), Convert.ToInt32(Math.Ceiling(textbox.Height + z)));
            textImage.Mutate(ctx =>
            {
                ctx.DrawText(new RichTextOptions(f)
                {
                    FallbackFontFamilies = fallbacks
                }, text, color);
                ctx.Resize(Convert.ToInt32(Math.Ceiling(textImage.Width * size / z)), Convert.ToInt32(Math.Ceiling(textImage.Height * size / z)), resampler);
            });

            return textImage;
        }
        public Image DrawImage(float size, string text, Brush brush, Pen pen, IReadOnlyList<FontFamily> fallbacks, IResampler resampler)
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

            Image textImage = new Image<Rgba32>(Convert.ToInt32(Math.Ceiling(textbox.Width + z)), Convert.ToInt32(Math.Ceiling(textbox.Height + z)));
            textImage.Mutate(ctx =>
            {
                ctx.DrawText(new RichTextOptions(f)
                {
                    FallbackFontFamilies = fallbacks
                }, text, brush, pen);
                ctx.Resize(Convert.ToInt32(Math.Ceiling(textImage.Width * size / z)), Convert.ToInt32(Math.Ceiling(textImage.Height * size / z)), resampler);
            });

            return textImage;
        }
    }

    extension(Image image)
    {
        public void Resize(int width, int height, IResampler resampler) =>
            image.Mutate(ctx => ctx.Resize(width, height, resampler));

        public void Resize(double percent, IResampler resampler) =>
            image.Mutate(ctx => ctx.Resize(Convert.ToInt32(Math.Ceiling(image.Width * percent)), Convert.ToInt32(Math.Ceiling(image.Height * percent)), resampler));
    }
}
