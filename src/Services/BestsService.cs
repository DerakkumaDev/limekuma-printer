using Limekuma.Prober.Common;
using Limekuma.Utils;

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
}