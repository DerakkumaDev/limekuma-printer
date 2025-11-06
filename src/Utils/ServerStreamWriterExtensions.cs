using Google.Protobuf;
using Grpc.Core;
using LimeKuma;
using SixLabors.ImageSharp;
#if RELEASE
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Processing;
#endif

namespace Limekuma.Utils;

internal static class ServerStreamWriterExtensions
{
    private const int ChunkSize = 1024 * 32;

    extension(IServerStreamWriter<ImageResponse> responseStream)
    {
        internal async Task WriteToResponseAsync(Image image) =>
            await responseStream.WriteToResponseAsync(image, false);

        internal async Task WriteToResponseAsync(Image image, bool isAnime)
        {
            MemoryStream outStream = new();
#if RELEASE
            if (image.Height > image.Width)
            {
                image.Mutate(ctx => ctx.Resize(1080, image.Height / image.Width * 1080));
            }
            else
            {
                image.Mutate(ctx => ctx.Resize(image.Width / image.Height * 1080, 1080));
            }

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