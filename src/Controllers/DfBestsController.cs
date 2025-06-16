using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Limekuma.Controllers;

public partial class BestsController : BaseController
{
    private static async Task<(CommonUser, List<CommonRecord>, List<CommonRecord>, int, int)> PrepareDfDataAsync(
        uint qq, int frame = 200502, int plate = 101, int icon = 101)
    {
        DfResourceClient df = new();
        Player player = await df.GetPlayerAsync(qq);

        CommonUser user = player;
        user.FrameId = frame;
        user.PlateId = plate;
        user.IconId = icon;

        List<CommonRecord> bestEver = player.Bests.Ever.ConvertAll<CommonRecord>(_ => _);
        bestEver.SortRecordForBests();
        int everTotal = bestEver.Sum(x => x.DXRating);

        List<CommonRecord> bestCurrent = player.Bests.Current.ConvertAll<CommonRecord>(_ => _);
        bestCurrent.SortRecordForBests();
        int currentTotal = bestCurrent.Sum(x => x.DXRating);

        await PrepareDataAsync(user, bestEver, bestCurrent);

        return (user, bestEver, bestCurrent, everTotal, currentTotal);
    }

    [HttpGet("diving-fish")]
    public async Task<IActionResult> GetDivingFishBestsAsync([FromQuery] uint qq, [FromQuery] int frame = 200502,
        [FromQuery] int plate = 101, [FromQuery] int icon = 101)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfDataAsync(qq, frame, plate, icon);
        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal);

        return await ReturnImageAsync(bestsImage);
    }

    [HttpGet("anime/diving-fish")]
    public async Task<IActionResult> GetDivingFishBestsAnimationAsync([FromQuery] uint qq,
        [FromQuery] int frame = 200502, [FromQuery] int plate = 101, [FromQuery] int icon = 101)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfDataAsync(qq, frame, plate, icon);
        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal,
            BestsDrawer.BackgroundAnimationPath);

        return await ReturnImageAsync(bestsImage, true);
    }
}