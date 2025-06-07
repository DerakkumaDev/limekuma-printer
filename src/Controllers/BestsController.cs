using Limekuma.Prober.Common;
using Microsoft.AspNetCore.Mvc;

namespace Limekuma.Controllers;

[Route("bests")]
public partial class BestsController : BaseController
{
    protected static async Task PrepareDataAsync(CommonUser user, List<CommonRecord> bestsEver,
        List<CommonRecord> bestsCurrent)
    {
        await PrepareUserDataAsync(user);
        await PrepareRecordDataAsync(bestsEver);
        await PrepareRecordDataAsync(bestsCurrent);
    }
}