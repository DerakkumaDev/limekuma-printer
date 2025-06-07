using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Limekuma.Controllers;

public partial class ListController : BaseController
{
    [HttpGet("diving-fish")]
    public async Task<IActionResult> GetDivingFishListAsync([FromQuery(Name = "dev-token")] string token,
        [FromQuery] uint qq, [FromQuery] string level, [FromQuery] int page = 1, [FromQuery] int plate = 101)
    {
        DfDeveloperClient df = new(token);
        PlayerData player = await df.GetPlayerDataAsync(qq);
        CommonUser user = player;
        user.PlateId = plate;

        List<Record> records = [.. player.Records.Where(x => x.Level == level)];
        int count = records.Count;
        List<CommonRecord> cRecords = records.ConvertAll<CommonRecord>(_ => _);
        cRecords.SortRecordForList();
        (int[] counts, int startIndex, int endIndex) = await PrepareDataAsync(user, cRecords, page);
        int total = (int)Math.Ceiling((double)count / 55);

        using Image bestsImage =
            new ListDrawer().Draw(user, cRecords[startIndex..endIndex], page, total, counts, level);

        return await ReturnImageAsync(bestsImage);
    }
}