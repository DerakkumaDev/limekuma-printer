using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Limekuma.Controllers;

public partial class BestsController : ControllerBase
{
    private static async Task<(CommonUser, List<CommonRecord>, List<CommonRecord>, int, int)> PrepareDfData(uint qq,
        int frame = 200502, int plate = 101)
    {
        DfResourceClient df = new();
        Player player = await df.GetPlayerAsync(qq);
        CommonUser user = player;
        user.FrameId = frame;
        user.PlateId = plate;

        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.IconRootPath, $"{user.IconId}.png")))
        {
            using HttpClient http = new();
            using FileStream stream =
                System.IO.File.OpenWrite(Path.Combine(BestsDrawer.IconRootPath, $"{user.IconId}.png"));
            (await http.GetStreamAsync(user.IconUrl)).CopyTo(stream);
        }

        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.PlateRootPath,
                $"{user.PlateId.ToString().PadLeft(6, '0')}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(BestsDrawer.PlateRootPath,
                $"{user.PlateId.ToString().PadLeft(6, '0')}.png"));
            (await http.GetStreamAsync(user.PlateUrl)).CopyTo(stream);
        }

        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.FrameRootPath,
                $"UI_Frame_{user.FrameId.ToString().PadLeft(6, '0')}.png")))
        {
            user.FrameId = 200502;
        }

        player.Bests.Ever.SortRecordForBests();
        player.Bests.Current.SortRecordForBests();

        List<CommonRecord> bestEver = [];
        int everTotal = 0;

        foreach (Record record in player.Bests.Ever)
        {
            bestEver.Add(record);
            everTotal += record.DXRating;
            if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream =
                System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png"));
            (await http.GetStreamAsync(record.JacketUrl)).CopyTo(stream);
        }

        List<CommonRecord> bestCurrent = [];
        int currentTotal = 0;

        foreach (Record record in player.Bests.Current)
        {
            bestCurrent.Add(record);
            currentTotal += record.DXRating;
            if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream =
                System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png"));
            (await http.GetStreamAsync(record.JacketUrl)).CopyTo(stream);
        }

        return (user, bestEver, bestCurrent, everTotal, currentTotal);
    }

    [HttpGet("diving-fish")]
    public async Task<IActionResult> GetDivingFishBests([FromQuery] uint qq, [FromQuery] int frame = 200502,
        [FromQuery] int plate = 101)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfData(qq, frame, plate);
        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal);

        MemoryStream outStream = new();
        await bestsImage.SaveAsJpegAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/jpeg");
    }

    [HttpGet("anime/diving-fish")]
    public async Task<IActionResult> GetDivingFishBestsAnimation([FromQuery] uint qq, [FromQuery] int frame = 200502,
        [FromQuery] int plate = 101)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfData(qq, frame, plate);
        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal,
            BestsDrawer.BackgroundAnimationPath);

        MemoryStream outStream = new();
        await bestsImage.SaveAsGifAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/gif");
    }
}