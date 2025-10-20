using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Utils;
using LimeKuma;

namespace Limekuma.Services;

public sealed partial class ListService : ListApi.ListApiBase
{
    private static async Task<(int[], int, int)> PrepareDataAsync(CommonUser user, List<CommonRecord> records,
        int page = 1)
    {
        int i = (page - 1) * 55;
        int count = records.Count;
        if (i >= count)
        {
            throw new RpcException(new(StatusCode.OutOfRange, "The page number is out of the range boundary."));
        }

        await ServiceHelper.PrepareUserDataAsync(user);

        int[] counts = new int[16];
        counts[15] = count;

        int j = i;
        for (int k = Math.Min(i + 55, count); j < k; ++j)
        {
            CommonRecord record = records[j];
            await ServiceHelper.PrepareRecordDataAsync(record, CancellationToken.None);
        }

        Parallel.ForEach(records, record =>
        {
            if (record.Rank >= Ranks.S)
            {
                ++counts[record.Rank switch
                {
                    Ranks.SSSPlus => 0,
                    Ranks.SSS => 1,
                    Ranks.SSPlus => 2,
                    Ranks.SS => 3,
                    Ranks.SPlus => 4,
                    Ranks.S => 5,
                    _ => 16
                }];
            }

            if (record.Rank >= Ranks.A)
            {
                ++counts[6];
            }

            if (record.ComboFlag >= ComboFlags.FullCombo)
            {
                ++counts[record.ComboFlag switch
                {
                    ComboFlags.AllPerfectPlus => 7,
                    ComboFlags.AllPerfect => 8,
                    ComboFlags.FullComboPlus => 9,
                    ComboFlags.FullCombo => 10,
                    _ => 16
                }];
            }

            if (record.SyncFlag is >= SyncFlags.FullSync and <= SyncFlags.FullSyncDXPlus)
            {
                ++counts[record.SyncFlag switch
                {
                    SyncFlags.FullSyncDXPlus => 11,
                    SyncFlags.FullSyncDX => 12,
                    SyncFlags.FullSyncPlus => 13,
                    SyncFlags.FullSync => 14,
                    _ => 16
                }];
            }
        });

        return (counts, i, j);
    }
}