using Google.Protobuf;
using Grpc.Core;
using LimeKuma;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Limekuma.Utils;

internal static class DrawExtensions
{
    private const int ChunkSize = 1024 * 32;

    extension(FontFamily font)
    {
        internal Font GetSizeFont(float size) => new(font, size);

        internal Image DrawImage(float size, string text, Color color, IReadOnlyList<FontFamily> fallbacks,
            IResampler resampler) => font.DrawImage(size, text, color, fallbacks, resampler, 5);

        internal Image DrawImage(float size, string text, Color color, IReadOnlyList<FontFamily> fallbacks,
            IResampler resampler, int zoom)
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
            IResampler resampler) => font.DrawImage(size, text, brush, pen, fallbacks, resampler, 5);

        internal Image DrawImage(float size, string text, Brush brush, Pen pen, IReadOnlyList<FontFamily> fallbacks,
            IResampler resampler, int zoom)
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

        internal async Task WriteToResponseAsync(IServerStreamWriter<ImageResponse> responseStream) =>
            await image.WriteToResponseAsync(responseStream, false);

        internal async Task WriteToResponseAsync(IServerStreamWriter<ImageResponse> responseStream, bool isAnime)
        {
            MemoryStream outStream = new();
#if RELEASE
            image.Resize(1080, 1920, KnownResamplers.Lanczos3);
            if (isAnime)
            {
                GifEncoder encoder = new()
                {
                    Quantizer = KnownQuantizers.Wu,
                    ColorTableMode = GifColorTableMode.Local
                };
                await image.SaveAsGifAsync(outStream, encoder);
            }
            else
            {
                await image.SaveAsJpegAsync(outStream);
            }

#elif DEBUG
            await image.SaveAsPngAsync(outStream);
#endif
            outStream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[ChunkSize];

            while (true)
            {
                int numBytesRead = await outStream.ReadAsync(buffer);
                if (numBytesRead is 0)
                {
                    break;
                }

                await responseStream.WriteAsync(new()
                {
                    Data = UnsafeByteOperations.UnsafeWrap(buffer.AsMemory(0, numBytesRead))
                });
            }
        }
    }
}