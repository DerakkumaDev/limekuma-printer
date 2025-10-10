using Grpc.Core;
using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using LimeKuma;
using SixLabors.ImageSharp;
using System.Net;

namespace Limekuma.Services;

public partial class BestsService
{
    private static async Task<(CommonUser, List<CommonRecord>, List<CommonRecord>, int, int)> PrepareDfDataAsync(
        uint qq, int frame = 558001, int plate = 458001, int icon = 458001)
    {
        DfResourceClient df = new();
        Player player;
        try
        {
            player = await df.GetPlayerAsync(qq);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.BadRequest)
        {
            throw new RpcException(new(StatusCode.NotFound, ex.Message, ex));
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden)
        {
            throw new RpcException(new(StatusCode.PermissionDenied, ex.Message, ex));
        }

        CommonUser user = player;
        user.FrameId = frame;
        user.PlateId = plate;
        user.IconId = icon;

        List<CommonRecord> bestEver = player.Bests.Ever.ConvertAll<CommonRecord>(_ => _);
        bestEver.SortRecordForBests();
        int everTotal = bestEver.Sum(x => x.DXRating);

        List<CommonRecord> bestCurrent = player.Bests.Current.ConvertAll<CommonRecord>(_ => _);
        bestCurrent.SortRecordForBests();
        int currentTotal = bestCurrent.Sum(x => x.DXRating);

        await PrepareDataAsync(user, bestEver, bestCurrent);

        return (user, bestEver, bestCurrent, everTotal, currentTotal);
    }

    public override async Task GetFromDivingFish(DivingFishBestsRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfDataAsync(request.Qq, request.Frame, request.Plate, request.Icon);
        using Image bestsImage =
            new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal, "Best 50");

        await bestsImage.WriteToResponseAsync(responseStream);
    }

    public override async Task GetAnimeFromDivingFish(DivingFishBestsRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfDataAsync(request.Qq, request.Frame, request.Plate, request.Icon);
        using Image bestsImage = new BestsDrawer().Draw(user, bestEver, bestCurrent, everTotal, currentTotal, "Best 50", true);

        await bestsImage.WriteToResponseAsync(responseStream, true);
    }
}