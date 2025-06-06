using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;

namespace Limekuma.Controllers;

public partial class ListController : ControllerBase
{
    private static async Task<(CommonUser, List<CommonRecord>, int, int[])> PrepareDfData(string token, uint qq, string level, int page = 1, int frame = 200502, int plate = 101)
    {
        DfDeveloperClient df = new(token);
        Player player = await df.GetAllRecordsAsync(qq);
        CommonUser user = player;
        user.FrameId = frame;
        user.PlateId = plate;

        player.Records.Sort((x, y) =>
        {
            int compare = x.DXRating.CompareTo(y.DXRating);
            if (compare is not 0)
            {
                return compare;
            }

            compare = x.LevelValue.CompareTo(y.LevelValue);
            if (compare is not 0)
            {
                return compare;
            }

            compare = x.Achievements.CompareTo(y.Achievements);
            if (compare is not 0)
            {
                return compare;
            }

            return 0;
        });
        List<Record> records = [.. player.Records.Where(x => x.Level == level)];
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

            if (record.ComboFlag.Value is ComboFlags comboFlag && comboFlag >= ComboFlags.FullCombo)
            {
                ++counts[comboFlag switch
                {
                    ComboFlags.AllPerfectPlus => 7,
                    ComboFlags.AllPerfect => 8,
                    ComboFlags.FullComboPlus => 9,
                    ComboFlags.FullCombo => 10,
                    _ => 15
                }];
            }

            if (record.SyncFlag.Value is SyncFlags syncFlag && syncFlag is >= SyncFlags.FullSync and <= SyncFlags.FullSyncDXPlus)
            {
                ++counts[syncFlag switch
                {
                    SyncFlags.FullSync => 11,
                    SyncFlags.FullSyncPlus => 12,
                    SyncFlags.FullSyncDX => 13,
                    SyncFlags.FullSyncDXPlus => 14,
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

    [HttpGet("diving-fish")]
    public async Task<IActionResult> GetDivingFishList([FromQuery] string token, [FromQuery] uint qq, [FromQuery] string level, [FromQuery] int page = 1, [FromQuery] int frame = 200502, [FromQuery] int plate = 101)
    {
        (CommonUser user, List<CommonRecord> records, int count, int[] counts) = await PrepareDfData(token, qq, level, page, frame, plate);

        int total = (int)Math.Ceiling((double)count / 50);
        using Image bestsImage = new ListDrawer().Draw(user, records, page, total, counts, level, ListDrawer.BackgroundPath);

        MemoryStream outStream = new();
        await bestsImage.SaveAsJpegAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/jpeg");
    }
}
