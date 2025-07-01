using Limekuma.Prober.Common;
using Limekuma.Utils;
using LimeKuma;

namespace Limekuma.Services;

public sealed partial class BestsService : BestsApi.BestsApiBase
{
    private static async Task PrepareDataAsync(CommonUser user, IList<CommonRecord> bestsEver,
        IList<CommonRecord> bestsCurrent)
    {
        await ServiceHelper.PrepareUserDataAsync(user);
        await ServiceHelper.PrepareRecordDataAsync(bestsEver);
        await ServiceHelper.PrepareRecordDataAsync(bestsCurrent);
    }
}