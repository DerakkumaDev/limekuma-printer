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

        int[] counts = new int[15];

        int end = Math.Min(i + 55, count);
        await ServiceHelper.PrepareRecordDataAsync(records[i..end]);

        Parallel.ForEach(records, record =>
        {
            if (record.Rank >= Ranks.SSSPlus)
            {
                Interlocked.Increment(ref counts[0]);
            }

            if (record.Rank >= Ranks.SSS)
            {
                Interlocked.Increment(ref counts[1]);
            }

            if (record.Rank >= Ranks.SSPlus)
            {
                Interlocked.Increment(ref counts[2]);
            }

            if (record.Rank >= Ranks.SS)
            {
                Interlocked.Increment(ref counts[3]);
            }

            if (record.Rank >= Ranks.SPlus)
            {
                Interlocked.Increment(ref counts[4]);
            }

            if (record.Rank >= Ranks.S)
            {
                Interlocked.Increment(ref counts[5]);
            }

            if (record.Rank >= Ranks.A)
            {
                Interlocked.Increment(ref counts[6]);
            }

            if (record.ComboFlag >= ComboFlags.AllPerfectPlus)
            {
                Interlocked.Increment(ref counts[7]);
            }

            if (record.ComboFlag >= ComboFlags.AllPerfect)
            {
                Interlocked.Increment(ref counts[8]);
            }

            if (record.ComboFlag >= ComboFlags.FullComboPlus)
            {
                Interlocked.Increment(ref counts[9]);
            }

            if (record.ComboFlag >= ComboFlags.FullCombo)
            {
                Interlocked.Increment(ref counts[10]);
            }

            if (record.SyncFlag is SyncFlags.SyncPlay)
            {
                return;
            }

            if (record.SyncFlag >= SyncFlags.FullSyncDXPlus)
            {
                Interlocked.Increment(ref counts[11]);
            }

            if (record.SyncFlag >= SyncFlags.FullSyncDX)
            {
                Interlocked.Increment(ref counts[12]);
            }

            if (record.SyncFlag >= SyncFlags.FullSyncPlus)
            {
                Interlocked.Increment(ref counts[13]);
            }

            if (record.SyncFlag >= SyncFlags.FullSync)
            {
                Interlocked.Increment(ref counts[14]);
            }
        });

        return (counts, i, end);
    }
}