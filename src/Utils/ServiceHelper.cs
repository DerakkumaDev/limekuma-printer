using Google.Protobuf;
using Limekuma.Draw;
using Limekuma.Prober.Common;
using SixLabors.ImageSharp;

namespace Limekuma.Utils;

public static class ServiceHelper
{
    public static async Task PrepareUserDataAsync(CommonUser user)
    {
        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.IconRootPath, $"{user.IconId}.png")))
        {
            using HttpClient http = new();
            using FileStream stream =
                System.IO.File.OpenWrite(Path.Combine(BestsDrawer.IconRootPath, $"{user.IconId}.png"));
            using Stream imageStream = await http.GetStreamAsync(user.IconUrl);
            imageStream.CopyTo(stream);
        }

        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.PlateRootPath,
                $"{user.PlateId}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(BestsDrawer.PlateRootPath,
                $"{user.PlateId}.png"));
            using Stream imageStream = await http.GetStreamAsync(user.PlateUrl);
            imageStream.CopyTo(stream);
        }

        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.FrameRootPath,
                $"UI_Frame_{user.FrameId.ToString().PadLeft(6, '0')}.png")))
        {
            user.FrameId = 200502;
        }
    }

    public static async Task PrepareRecordDataAsync(IList<CommonRecord> records)
    {
        foreach (CommonRecord record in records)
        {
            await PrepareRecordDataAsync(record);
        }
    }

    public static async Task PrepareRecordDataAsync(CommonRecord record)
    {
        if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png")))
        {
            return;
        }

        using HttpClient http = new();
        using FileStream stream = System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png"));
        using Stream imageStream = await http.GetStreamAsync(record.JacketUrl);
        imageStream.CopyTo(stream);
    }

    public static async Task<ByteString> ReturnImageAsync(Image image, bool isAnime = false)
    {
        MemoryStream outStream = new();
        if (isAnime)
        {
            await image.SaveAsGifAsync(outStream);
        }
        else
        {
#if RELEASE
            await image.SaveAsJpegAsync(outStream);
#elif DEBUG
            await image.SaveAsPngAsync(outStream);
#endif
        }

        outStream.Seek(0, SeekOrigin.Begin);
        return await ByteString.FromStreamAsync(outStream);
    }
}