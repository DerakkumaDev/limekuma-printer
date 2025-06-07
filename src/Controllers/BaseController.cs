using Limekuma.Draw;
using Limekuma.Prober.Common;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Limekuma.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected static async Task PrepareUserDataAsync(CommonUser user)
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
                $"{user.PlateId.ToString().PadLeft(6, '0')}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(BestsDrawer.PlateRootPath,
                $"{user.PlateId.ToString().PadLeft(6, '0')}.png"));
            using Stream imageStream = await http.GetStreamAsync(user.PlateUrl);
            imageStream.CopyTo(stream);
        }

        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.FrameRootPath,
                $"UI_Frame_{user.FrameId.ToString().PadLeft(6, '0')}.png")))
        {
            user.FrameId = 200502;
        }
    }

    protected static async Task PrepareRecordDataAsync(List<CommonRecord> records)
    {
        foreach (CommonRecord record in records)
        {
            await PrepareRecordDataAsync(record);
        }
    }

    protected static async Task PrepareRecordDataAsync(CommonRecord record)
    {
        if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png")))
        {
            return;
        }

        using HttpClient http = new();
        using FileStream stream = System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png"));
        using Stream imageStream = await http.GetStreamAsync(record.JacketUrl);
        imageStream.CopyTo(stream);
    }

    protected async Task<IActionResult> ReturnImageAsync(Image image, bool isAnime = false)
    {
        MemoryStream outStream = new();
        string format;
        if (isAnime)
        {
            await image.SaveAsGifAsync(outStream);
            format = "gif";
        }
        else
        {
#if RELEASE
            await image.SaveAsJpegAsync(outStream);
            format = "jpeg";
#elif DEBUG
            await image.SaveAsPngAsync(outStream);
            format = "png";
#endif
        }

        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, $"image/{format}");
    }
}