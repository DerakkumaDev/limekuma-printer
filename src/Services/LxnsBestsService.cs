using Grpc.Core;
using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using Limekuma.Utils;
using SixLabors.ImageSharp;

namespace Limekuma.Services;

public partial class BestsService
{
    private static async Task<(CommonUser, List<CommonRecord>, List<CommonRecord>, int, int)> PrepareLxnsDataAsync(
        string devToken, uint? qq, string? personalToken)
    {
        Player player;
        LxnsDeveloperClient lxnsDev = new(devToken);
        if (qq.HasValue)
        {
            player = await lxnsDev.GetPlayerByQQAsync(qq.Value);
        }
        else if (!string.IsNullOrEmpty(personalToken))
        {
            LxnsPersonalClient lxns = new(personalToken);
            player = await lxns.GetPlayerAsync();
            player.Client = lxnsDev;
        }
        else
        {
            throw new InvalidOperationException();
        }

        if (player is null || player.FriendCode == default)
        {
            throw new KeyNotFoundException();
        }

        Bests bests = await player.GetBestsAsync();
        CommonUser user = player;

        List<CommonRecord> bestEver = bests.Ever.ConvertAll<CommonRecord>(_ => _);
        bestEver.SortRecordForBests();

        List<CommonRecord> bestCurrent = bests.Current.ConvertAll<CommonRecord>(_ => _);
        bestCurrent.SortRecordForBests();

        await PrepareDataAsync(user, bestEver, bestCurrent);

        return (user, bestEver, bestCurrent, bests.EverTotal, bests.CurrentTotal);
    }

    public override async Task<ImageReply> GetFromLxns(LxnsBestsRequest request, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareLxnsDataAsync(request.DevToken, request.Qq, request.PersonalToken);
        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal);

        return new()
        {
            Image = await ServiceHelper.ReturnImageAsync(bestsImage)
        };
    }

    public override async Task<ImageReply> GetAnimeFromLxns(LxnsBestsRequest request, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareLxnsDataAsync(request.DevToken, request.Qq, request.PersonalToken);
        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal,
            BestsDrawer.BackgroundAnimationPath);

        return new()
        {
            Image = await ServiceHelper.ReturnImageAsync(bestsImage, true)
        };
    }
}
