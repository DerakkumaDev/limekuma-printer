using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.Services;

public sealed partial class ListService : ListApi.ListApiBase
{
    private static (ImmutableArray<CommonRecord> Records, bool MayMask) BuildListRecords(IReadOnlyList<string> tags,
        string condition, IEnumerable<CommonRecord> records)
    {
        (Func<CommonRecord, bool> predicate, bool maskMutex) = ScoreFilterHelper.GetPredicateByTags(tags, condition);
        bool mayMask = ServiceExecutionHelper.HasMaskedScores(records);
        ServiceExecutionHelper.EnsurePermission(!(mayMask && maskMutex), "Mask enabled");

        ImmutableArray<CommonRecord> filteredRecords = [.. records.Where(predicate).SortRecordForList()];
        return (filteredRecords, mayMask);
    }

    private static async Task<(ImmutableArray<int>, int, int)> PrepareDataAsync(CommonUser user,
        ImmutableArray<CommonRecord> records, int page)
    {
        int i = (page - 1) * 55;
        int count = records.Length;
        if (i >= count)
        {
            throw new RpcException(new(StatusCode.OutOfRange, "The page number is out of the range boundary."));
        }

        await ServiceHelper.PrepareUserDataAsync(user);

        int end = Math.Min(i + 55, count);
        await ServiceHelper.PrepareRecordDataAsync(records[i..end]);

        int[] counts = records.AsParallel().Aggregate(() => new int[15], (left, record) =>
        {
            int[] right = CountRecordStats(record);
            for (int idx = 0; idx < left.Length; idx++)
            {
                left[idx] += right[idx];
            }

            return left;
        }, (left, right) =>
        {
            for (int idx = 0; idx < left.Length; idx++)
            {
                left[idx] += right[idx];
            }

            return left;
        }, localCounts => localCounts);

        return ([.. counts], i, end);
    }

    private static int[] CountRecordStats(CommonRecord record)
    {
        int[] counts = new int[15];
        if (record.Rank >= Ranks.SSSPlus)
        {
            counts[0]++;
        }

        if (record.Rank >= Ranks.SSS)
        {
            counts[1]++;
        }

        if (record.Rank >= Ranks.SSPlus)
        {
            counts[2]++;
        }

        if (record.Rank >= Ranks.SS)
        {
            counts[3]++;
        }

        if (record.Rank >= Ranks.SPlus)
        {
            counts[4]++;
        }

        if (record.Rank >= Ranks.S)
        {
            counts[5]++;
        }

        if (record.Rank >= Ranks.A)
        {
            counts[6]++;
        }

        if (record.ComboFlag >= ComboFlags.AllPerfectPlus)
        {
            counts[7]++;
        }

        if (record.ComboFlag >= ComboFlags.AllPerfect)
        {
            counts[8]++;
        }

        if (record.ComboFlag >= ComboFlags.FullComboPlus)
        {
            counts[9]++;
        }

        if (record.ComboFlag >= ComboFlags.FullCombo)
        {
            counts[10]++;
        }

        if (record.SyncFlag is SyncFlags.SyncPlay)
        {
            return counts;
        }

        if (record.SyncFlag >= SyncFlags.FullSyncDXPlus)
        {
            counts[11]++;
        }

        if (record.SyncFlag >= SyncFlags.FullSyncDX)
        {
            counts[12]++;
        }

        if (record.SyncFlag >= SyncFlags.FullSyncPlus)
        {
            counts[13]++;
        }

        if (record.SyncFlag >= SyncFlags.FullSync)
        {
            counts[14]++;
        }

        return counts;
    }
}
