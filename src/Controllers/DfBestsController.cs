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

        List<CommonRecord> bestEver = player.Bests.Ever.ConvertAll<CommonRecord>(_ => _);
        bestEver.SortRecordForBests();
        int everTotal = bestEver.Sum(x => x.DXRating);

        List<CommonRecord> bestCurrent = player.Bests.Current.ConvertAll<CommonRecord>(_ => _);
        bestCurrent.SortRecordForBests();
        int currentTotal = bestCurrent.Sum(x => x.DXRating);

        await PrepareData(user, bestEver, bestCurrent);

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
#if RELEASE
        await bestsImage.SaveAsJpegAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/jpeg");
#elif DEBUG
        await bestsImage.SaveAsPngAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/png");
#endif
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