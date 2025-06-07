using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using Limekuma.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Limekuma.Controllers;

public partial class ListController : BaseController
{
    [HttpGet("lxns")]
    public async Task<IActionResult> GetLxnsListAsync([FromQuery(Name = "personal-token")] string personalToken,
        [FromQuery] string level, [FromQuery] int page = 1)
    {
        LxnsPersonalClient lxns = new(personalToken);
        List<Record> records = await lxns.GetRecordsAsync();
        CommonUser user = await lxns.GetPlayerAsync();

        records = [.. records.Where(x => x.Level == level)];
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