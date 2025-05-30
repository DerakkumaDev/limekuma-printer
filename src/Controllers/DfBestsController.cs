using DXKumaBot.Backend.Prober.Common;
using DXKumaBot.Backend.Prober.DivingFish;
using DXKumaBot.Backend.Prober.DivingFish.Models;
using DXKumaBot.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using static DXKumaBot.Backend.Utils.Shared;

namespace DXKumaBot.Backend.Controllers;

public partial class BestsController : ControllerBase
{
    private static async Task<(CommonUser, List<CommonRecord>, List<CommonRecord>, int, int)> PrepareDfData(uint qq, int frame = 200502, int plate = 101)
    {
        DfResourceClient df = new();
        PlayerInfo player = await df.GetPlayerAsync(qq);
        CommonUser user = player;
        user.FrameId = frame;
        user.PlateId = plate;

        if (!System.IO.File.Exists(Path.Combine(IconRootPath, $"{user.IconId}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(IconRootPath, $"{user.IconId}.png"));
            http.GetStreamAsync(user.IconUrl).Result.CopyTo(stream);
        }

        if (!System.IO.File.Exists(Path.Combine(PlateRootPath, $"{user.PlateId.ToString().PadLeft(6, '0')}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(PlateRootPath, $"{user.PlateId.ToString().PadLeft(6, '0')}.png"));
            http.GetStreamAsync(user.PlateUrl).Result.CopyTo(stream);
        }

        if (!System.IO.File.Exists(Path.Combine(FrameRootPath, $"UI_Frame_{user.FrameId.ToString().PadLeft(6, '0')}.png")))
        {
            user.FrameId = 558001;
        }

        List<CommonRecord> bestEver = [];
        int everTotal = 0;

        foreach (Record record in player.Bests.Ever)
        {
            bestEver.Add(record);
            everTotal += record.DXRating;
            if (System.IO.File.Exists(Path.Combine(JacketRootPath, $"{record.Id}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(JacketRootPath, $"{record.Id}.png"));
            http.GetStreamAsync(record.JacketUrl).Result.CopyTo(stream);
        }

        List<CommonRecord> bestCurrent = [];
        int currentTotal = 0;

        foreach (Record record in player.Bests.Current)
        {
            bestCurrent.Add(record);
            currentTotal += record.DXRating;
            if (System.IO.File.Exists(Path.Combine(JacketRootPath, $"{record.Id}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(JacketRootPath, $"{record.Id}.png"));
            http.GetStreamAsync(record.JacketUrl).Result.CopyTo(stream);
        }

        return (user, bestEver, bestCurrent, everTotal, currentTotal);
    }

    [HttpGet("diving-fish")]
    public async Task<IActionResult> GetDivingFishBests([FromQuery] uint qq, [FromQuery] int frame = 200502, [FromQuery] int plate = 101)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal, int currentTotal) = await PrepareDfData(qq, frame, plate);

        using Image bestsImage = new Draw().DrawBests(user, bestEver, bestCurrent, everTotal, currentTotal, BackgroundPath);

        MemoryStream outStream = new();
        await bestsImage.SaveAsPngAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/png");
    }

    [HttpGet("animation/diving-fish")]
    public async Task<IActionResult> GetDivingFishBestsAnimation([FromQuery] uint qq, [FromQuery] int frame = 200502, [FromQuery] int plate = 101)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal, int currentTotal) = await PrepareDfData(qq, frame, plate);

        using Image bestsImage = new Draw().DrawBests(user, bestEver, bestCurrent, everTotal, currentTotal, BackgroundAnimationPath);

        MemoryStream outStream = new();
        await bestsImage.SaveAsGifAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/gif");
    }
}