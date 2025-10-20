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
        int end = Math.Min(i + 55, count);
        await ServiceHelper.PrepareRecordDataAsync(records[i..end]);
        j = end;

        Parallel.ForEach(
            source: records,
            localInit: () => new int[16],
            body: (record, _, local) =>
            {
                if (record.Rank >= Ranks.S)
                {
                    ++local[record.Rank switch
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
                    ++local[6];
                }

                if (record.ComboFlag >= ComboFlags.FullCombo)
                {
                    ++local[record.ComboFlag switch
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
                    ++local[record.SyncFlag switch
                    {
                        SyncFlags.FullSyncDXPlus => 11,
                        SyncFlags.FullSyncDX => 12,
                        SyncFlags.FullSyncPlus => 13,
                        SyncFlags.FullSync => 14,
                        _ => 16
                    }];
                }

                return local;
            },
            localFinally: local =>
            {
                for (int idx = 0; idx < local.Length; ++idx)
                {
                    if (idx < counts.Length)
                    {
                        Interlocked.Add(ref counts[idx], local[idx]);
                    }
                }
            });

        return (counts, i, j);
    }
}