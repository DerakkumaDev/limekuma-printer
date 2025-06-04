using DXKumaBot.Backend.Prober.Common;
using DXKumaBot.Backend.Prober.Lxns;
using DXKumaBot.Backend.Prober.Lxns.Models;
using DXKumaBot.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using static DXKumaBot.Backend.Utils.Shared;

namespace DXKumaBot.Backend.Controllers;

[ApiController]
[Route("bests")]
public partial class BestsController : ControllerBase
{
    private static async Task<(CommonUser, List<CommonRecord>, List<CommonRecord>, int, int)> PrepareLxnsData(string token, uint qq)
    {
        LxnsDeveloperClient lxns = new(token);
        Player? player = await lxns.GetPlayerByQQAsync(qq);

        if (player is null || player.FriendCode == default)
        {
            throw new KeyNotFoundException();
        }

        Bests bests = await player.GetBestsAsync();
        CommonUser user = player;

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
            user.FrameId = 200502;
        }

        List<CommonRecord> bestEver = [];

        foreach (Record record in bests.Ever)
        {
            bestEver.Add(record);
            if (System.IO.File.Exists(Path.Combine(JacketRootPath, $"{record.Id}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(JacketRootPath, $"{record.Id}.png"));
            http.GetStreamAsync(record.JacketUrl).Result.CopyTo(stream);
        }

        List<CommonRecord> bestCurrent = [];

        foreach (Record record in bests.Current)
        {
            bestCurrent.Add(record);
            if (System.IO.File.Exists(Path.Combine(JacketRootPath, $"{record.Id}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(JacketRootPath, $"{record.Id}.png"));
            http.GetStreamAsync(record.JacketUrl).Result.CopyTo(stream);
        }

        return (user, bestEver, bestCurrent, bests.EverTotal, bests.CurrentTotal);
    }

    [HttpGet("lxns")]
    public async Task<IActionResult> GetLxnsBests([FromQuery] string token, [FromQuery] uint qq)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal, int currentTotal) = await PrepareLxnsData(token, qq);

        using Image bestsImage = new Draw().DrawBests(user, bestEver, bestCurrent, everTotal, currentTotal, BackgroundPath);

        MemoryStream outStream = new();
        await bestsImage.SaveAsJpegAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/jpeg");
    }

    [HttpGet("anime/lxns")]
    public async Task<IActionResult> GetLxnsBestsAnimation([FromQuery] string token, [FromQuery] uint qq)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal, int currentTotal) = await PrepareLxnsData(token, qq);

        using Image bestsImage = new Draw().DrawBests(user, bestEver, bestCurrent, everTotal, currentTotal, BackgroundAnimationPath);

        MemoryStream outStream = new();
        await bestsImage.SaveAsGifAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/gif");
    }
} 