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
    private static async Task<(CommonUser, List<CommonRecord>, int, int[])> PrepareDfData(string token, uint qq,
        string level, int page = 1, int plate = 101)
    {
        DfDeveloperClient df = new(token);
        PlayerData player = await df.GetPlayerDataAsync(qq);
        CommonUser user = player;
        user.PlateId = plate;

        List<Record> records = [.. player.Records.Where(x => x.Level == level)];
        int i = (page - 1) * 55;
        int count = records.Count;
        if (i >= count)
        {
            throw new IndexOutOfRangeException();
        }

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

        records.SortRecordForList();

        List<CommonRecord> cRecords = [];
        int[] counts = new int[15];

        for (int j = i, k = i + 55; j < count && j < k; ++j)
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

            if (record.SyncFlag.Value is SyncFlags syncFlag &&
                syncFlag is >= SyncFlags.FullSync and <= SyncFlags.FullSyncDXPlus)
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

            if (System.IO.File.Exists(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png")))
            {
                continue;
            }

            using HttpClient http = new();
            using FileStream stream =
                System.IO.File.OpenWrite(Path.Combine(DrawerBase.JacketRootPath, $"{record.Id % 10000}.png"));
            (await http.GetStreamAsync(record.JacketUrl)).CopyTo(stream);
        }

        return (user, cRecords, count, counts);
    }

    [HttpGet("diving-fish")]
    public async Task<IActionResult> GetDivingFishList([FromQuery(Name = "dev-token")] string token,
        [FromQuery] uint qq, [FromQuery] string level, [FromQuery] int page = 1, [FromQuery] int plate = 101)
    {
        (CommonUser user, List<CommonRecord> records, int count, int[] counts) =
            await PrepareDfData(token, qq, level, page, plate);
        int total = (int)Math.Ceiling((double)count / 55);
        using Image bestsImage = new ListDrawer().Draw(user, records, page, total, counts, level);

        MemoryStream outStream = new();
        await bestsImage.SaveAsJpegAsync(outStream);
        outStream.Seek(0, SeekOrigin.Begin);
        return File(outStream, "image/jpeg");
    }
}