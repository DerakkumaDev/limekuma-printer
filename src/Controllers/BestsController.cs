using Limekuma.Draw;
using Limekuma.Prober.Common;
using Microsoft.AspNetCore.Mvc;

namespace Limekuma.Controllers;

[ApiController]
[Route("bests")]
public partial class BestsController : ControllerBase
{
    protected static async Task PrepareData(CommonUser user, List<CommonRecord> bestsEver, List<CommonRecord> bestsCurrent)
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

        foreach (CommonRecord record in bestsEver)
        {
            if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream =
                System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png"));
            using Stream imageStream = await http.GetStreamAsync(record.JacketUrl);
            imageStream.CopyTo(stream);
        }

        foreach (CommonRecord record in bestsCurrent)
        {
            if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream =
                System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png"));
            using Stream imageStream = await http.GetStreamAsync(record.JacketUrl);
            imageStream.CopyTo(stream);
        }
    }
}