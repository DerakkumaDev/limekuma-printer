using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using Limekuma.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Limekuma.Controllers;

public partial class ListController : ControllerBase
{
    private static async Task<(CommonUser, List<CommonRecord>, int, int[])> PrepareLxnsData(string token, string level, int page = 1)
    {
        LxnsPersonalClient lxns = new(token);
        List<Record> records = await lxns.GetRecordsAsync();
        CommonUser user = await lxns.GetPlayerAsync();

        records = [.. records.Where(x => x.Level == level)];
        int i = (page - 1) * 50;
        int count = records.Count;
        if (i >= count)
        {
            throw new IndexOutOfRangeException();
        }

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

        List<CommonRecord> cRecords = [];
        int[] counts = new int[15];

        for (int j = i, k = i + 50; j < count && j < k; ++j)
        {
            Record record = records[j];
            cRecords.Add(record);
            if (record.Rank >= Ranks.A)
            {
                ++counts[record.Rank switch
                {
                    Ranks.SSSPlus => 0,
                    Ranks.SSS => 1,
                    Ranks.SSPlus => 2,
                    Ranks.SS => 3,
                    Ranks.SPlus => 4,
                    Ranks.S => 5,
                    >= Ranks.A => 6,
                    _ => 15
                }];
            }

            if (record.ComboFlag >= ComboFlags.FullCombo)
            {
                ++counts[record.ComboFlag switch
                {
                    ComboFlags.AllPerfectPlus => 7,
                    ComboFlags.AllPerfect => 8,
                    ComboFlags.FullComboPlus => 9,
                    ComboFlags.FullCombo => 10,
                    _ => 15
                }];
            }

            if (record.SyncFlag is >= SyncFlags.FullSync and <= SyncFlags.FullSyncDXPlus)
            {
                ++counts[record.SyncFlag switch
                {
                    SyncFlags.FullSync => 11,
                    SyncFlags.FullSyncPlus => 12,
                    SyncFlags.FullSyncDX => 13,
                    SyncFlags.FullSyncDXPlus  => 14,
                    _ => 15
                }];
            }

            if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream = System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id}.png"));
            http.GetStreamAsync(record.JacketUrl).Result.CopyTo(stream);
        }

        return (user, cRecords, count, counts);
    }

    [HttpGet("lxns")]
    public async Task<IActionResult> GetLxnsList([FromQuery(Name = "personal-token")] string personalToken, [FromQuery] string level, [FromQuery] int page = 1)
    {
        (CommonUser user, List<CommonRecord> records, int count, int[] counts) = await PrepareLxnsData(personalToken, level, page);

        int total = (int)Math.Ceiling((double)count / 50);
        using Image bestsImage = new ListDrawer().Draw(user, records, page, total, counts, level, ListDrawer.BackgroundPath);

        MemoryStream outStream = new();
        await bestsImage.SaveAsPngAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/png");
    }
}
