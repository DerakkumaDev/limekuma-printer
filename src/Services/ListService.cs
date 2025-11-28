using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Utils;

namespace Limekuma.Services;

public sealed partial class ListService : ListApi.ListApiBase
{
    private static async Task<(int[], int, int)> PrepareDataAsync(CommonUser user, List<CommonRecord> records, int page)
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

        Parallel.ForEach(records, () => new int[15], (record, _, local) =>
        {
            if (record.Rank >= Ranks.SSSPlus)
            {
                ++local[0];
            }

            if (record.Rank >= Ranks.SSS)
            {
                ++local[1];
            }

            if (record.Rank >= Ranks.SSPlus)
            {
                ++local[2];
            }

            if (record.Rank >= Ranks.SS)
            {
                ++local[3];
            }

            if (record.Rank >= Ranks.SPlus)
            {
                ++local[4];
            }

            if (record.Rank >= Ranks.S)
            {
                ++local[5];
            }

            if (record.Rank >= Ranks.A)
            {
                ++local[6];
            }

            if (record.ComboFlag >= ComboFlags.AllPerfectPlus)
            {
                ++local[7];
            }

            if (record.ComboFlag >= ComboFlags.AllPerfect)
            {
                ++local[8];
            }

            if (record.ComboFlag >= ComboFlags.FullComboPlus)
            {
                ++local[9];
            }

            if (record.ComboFlag >= ComboFlags.FullCombo)
            {
                ++local[10];
            }

            if (record.SyncFlag is SyncFlags.SyncPlay)
            {
                return local;
            }

            if (record.SyncFlag >= SyncFlags.FullSyncDXPlus)
            {
                ++local[11];
            }

            if (record.SyncFlag >= SyncFlags.FullSyncDX)
            {
                ++local[12];
            }

            if (record.SyncFlag >= SyncFlags.FullSyncPlus)
            {
                ++local[13];
            }

            if (record.SyncFlag >= SyncFlags.FullSync)
            {
                ++local[14];
            }

            return local;
        }, local =>
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