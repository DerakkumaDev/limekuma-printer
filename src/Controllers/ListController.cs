using Limekuma.Draw;
using Limekuma.Prober.Common;
using Microsoft.AspNetCore.Mvc;

namespace Limekuma.Controllers;

[Route("list")]
public partial class ListController : BaseController
{
    private static async Task<(int[], int, int)> PrepareDataAsync(CommonUser user, List<CommonRecord> records,
        int page = 1)
    {
        int i = (page - 1) * 55;
        int count = records.Count;
        if (i >= count)
        {
            throw new IndexOutOfRangeException();
        }

        await PrepareUserDataAsync(user);

        int[] counts = new int[15];

        int j = i;
        for (int k = Math.Min(i + 55, count); j < k; ++j)
        {
            CommonRecord record = records[j];
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
                    SyncFlags.FullSyncDXPlus => 14,
                    _ => 15
                }];
            }

            await PrepareRecordDataAsync(record);
        }

        return (counts, i, j);
    }
}