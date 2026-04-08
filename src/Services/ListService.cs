using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.Services;

public sealed partial class ListService : ListApi.ListApiBase
{
    private static async Task<(ImmutableArray<int>, int, int)> PrepareDataAsync(CommonUser user, ImmutableArray<CommonRecord> records, int page)
    {
        int i = (page - 1) * 55;
        int count = records.Length;
        if (i >= count)
        {
            throw new RpcException(new(StatusCode.OutOfRange, "The page number is out of the range boundary."));
        }

        await ServiceHelper.PrepareUserDataAsync(user);

        ImmutableArray<int>.Builder counts = ImmutableArray.CreateBuilder<int>(15);

        int end = Math.Min(i + 55, count);
        await ServiceHelper.PrepareRecordDataAsync(records[i..end]);

        Parallel.ForEach(records, record =>
        {
            if (record.Rank >= Ranks.SSSPlus)
            {
                ++counts[0];
            }

            if (record.Rank >= Ranks.SSS)
            {
                ++counts[1];
            }

            if (record.Rank >= Ranks.SSPlus)
            {
                ++counts[2];
            }

            if (record.Rank >= Ranks.SS)
            {
                ++counts[3];
            }

            if (record.Rank >= Ranks.SPlus)
            {
                ++counts[4];
            }

            if (record.Rank >= Ranks.S)
            {
                ++counts[5];
            }

            if (record.Rank >= Ranks.A)
            {
                ++counts[6];
            }

            if (record.ComboFlag >= ComboFlags.AllPerfectPlus)
            {
                ++counts[7];
            }

            if (record.ComboFlag >= ComboFlags.AllPerfect)
            {
                ++counts[8];
            }

            if (record.ComboFlag >= ComboFlags.FullComboPlus)
            {
                ++counts[9];
            }

            if (record.ComboFlag >= ComboFlags.FullCombo)
            {
                ++counts[10];
            }

            if (record.SyncFlag is SyncFlags.SyncPlay)
            {
                return;
            }

            if (record.SyncFlag >= SyncFlags.FullSyncDXPlus)
            {
                ++counts[11];
            }

            if (record.SyncFlag >= SyncFlags.FullSyncDX)
            {
                ++counts[12];
            }

            if (record.SyncFlag >= SyncFlags.FullSyncPlus)
            {
                ++counts[13];
            }

            if (record.SyncFlag >= SyncFlags.FullSync)
            {
                ++counts[14];
            }
        });

        return (counts.ToImmutable(), i, end);
    }
}