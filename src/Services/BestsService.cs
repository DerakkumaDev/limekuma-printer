using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Utils;
using System.Collections.Immutable;

namespace Limekuma.Services;

public sealed partial class BestsService : BestsApi.BestsApiBase
{
    private static async Task PrepareDataAsync(CommonUser user, IReadOnlyList<CommonRecord> bestsEver,
        IReadOnlyList<CommonRecord> bestsCurrent)
    {
        await ServiceHelper.PrepareUserDataAsync(user);
        await ServiceHelper.PrepareRecordDataAsync(bestsEver);
        await ServiceHelper.PrepareRecordDataAsync(bestsCurrent);
    }

    private static async
        Task<(ImmutableArray<CommonRecord> BestEver, ImmutableArray<CommonRecord> BestCurrent, int EverTotal, int
            CurrentTotal, CommonUser?)> ProcessBestsByTagsAsync(IReadOnlyList<string> tags, string condition,
            ImmutableArray<CommonRecord> records,
            Func<string, Task<(CommonUser, ImmutableArray<CommonRecord>)>> secondDataProvider)
    {
        if (ScoreProcesserHelper.GetProcesserByTags(tags) is not { } selectedProcesser)
        {
            throw new RpcException(new(StatusCode.InvalidArgument, "Invalid arguments"));
        }

        CommonUser? user2p = null;
        bool mayMask = ServiceExecutionHelper.HasMaskedScores(records);
        ServiceExecutionHelper.EnsurePermission(!(mayMask && selectedProcesser.MaskMutex), "Mask enabled");

        ImmutableArray<CommonRecord> bestEver;
        ImmutableArray<CommonRecord> bestCurrent;
        if (selectedProcesser.RequireSecondData)
        {
            (user2p, ImmutableArray<CommonRecord> records2p) = await secondDataProvider(condition);
            (bestEver, bestCurrent) = selectedProcesser.Processer.Process(records, records2p);
        }
        else
        {
            (Func<CommonRecord, bool> predicate, bool maskMutex) =
                ScoreFilterHelper.GetPredicateByTags(tags, condition);
            ServiceExecutionHelper.EnsurePermission(!(mayMask && maskMutex), "Mask enabled");

            ImmutableArray<CommonRecord> filteredRecords = [.. records.Where(predicate).SortRecordForBests()];
            (bestEver, bestCurrent) = selectedProcesser.Processer.Process(filteredRecords);
        }

        int everTotal = bestEver.Sum(x => x.DXRating);
        int currentTotal = bestCurrent.Sum(x => x.DXRating);
        return (bestEver, bestCurrent, everTotal, currentTotal, user2p);
    }
}
