using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using Limekuma.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Limekuma.Controllers;

public partial class BestsController : ControllerBase
{
    private static async Task<(CommonUser, List<CommonRecord>, List<CommonRecord>, int, int)> PrepareLxnsData(string devToken, uint? qq, string? importToken)
    {
        Player player;
        LxnsDeveloperClient lxnsDev = new(devToken);
        if (qq.HasValue)
        {
            player = await lxnsDev.GetPlayerByQQAsync(qq.Value);
        }
        else if (!string.IsNullOrEmpty(importToken))
        {
            LxnsPersonalClient lxns = new(importToken);
            player = await lxns.GetPlayerAsync();
            player.Client = lxnsDev;
        }
        else
        {
            throw new InvalidOperationException();
        }

        if (player is null || player.FriendCode == default)
        {
            throw new KeyNotFoundException();
        }

        Bests bests = await player.GetBestsAsync();
        CommonUser user = player;

        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.IconRootPath, $"{user.IconId}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(BestsDrawer.IconRootPath, $"{user.IconId}.png"));
            http.GetStreamAsync(user.IconUrl).Result.CopyTo(stream);
        }

        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.PlateRootPath, $"{user.PlateId.ToString().PadLeft(6, '0')}.png")))
        {
            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(BestsDrawer.PlateRootPath, $"{user.PlateId.ToString().PadLeft(6, '0')}.png"));
            http.GetStreamAsync(user.PlateUrl).Result.CopyTo(stream);
        }

        if (!System.IO.File.Exists(Path.Combine(BestsDrawer.FrameRootPath, $"UI_Frame_{user.FrameId.ToString().PadLeft(6, '0')}.png")))
        {
            user.FrameId = 200502;
        }

        List<CommonRecord> bestEver = [];

        foreach (Record record in bests.Ever)
        {
            bestEver.Add(record);
            if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png"));
            http.GetStreamAsync(record.JacketUrl).Result.CopyTo(stream);
        }

        List<CommonRecord> bestCurrent = [];

        foreach (Record record in bests.Current)
        {
            bestCurrent.Add(record);
            if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png"));
            http.GetStreamAsync(record.JacketUrl).Result.CopyTo(stream);
        }

        return (user, bestEver, bestCurrent, bests.EverTotal, bests.CurrentTotal);
    }

    [HttpGet("lxns")]
    public async Task<IActionResult> GetLxnsBests([FromQuery(Name = "dev-token")] string devToken, [FromQuery] uint? qq, [FromQuery(Name = "import-token")] string? importToken)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal, int currentTotal) = await PrepareLxnsData(devToken, qq, importToken);

        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal, BestsDrawer.BackgroundPath);

        MemoryStream outStream = new();
        await bestsImage.SaveAsJpegAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/jpeg");
    }

    [HttpGet("anime/lxns")]
    public async Task<IActionResult> GetLxnsBestsAnimation([FromQuery(Name = "dev-token")] string devToken, [FromQuery] uint? qq, [FromQuery(Name = "import-token")] string? importToken)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal, int currentTotal) = await PrepareLxnsData(devToken, qq, importToken);

        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal, BestsDrawer.BackgroundAnimationPath);

        MemoryStream outStream = new();
        await bestsImage.SaveAsGifAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/gif");
    }
}
