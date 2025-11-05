using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Render;
using Limekuma.Utils;
using LimeKuma;
using SixLabors.ImageSharp;
using System.Net;

namespace Limekuma.Services;

public partial class BestsService
{
    private static async Task<(CommonUser, List<CommonRecord>, List<CommonRecord>, int, int)> PrepareDfDataAsync(
        uint qq, int frame, int plate, int icon)
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
        using Image bestsImage = await new Drawer().DrawBestsAsync(user, bestEver, bestCurrent, everTotal, currentTotal,
            "水鱼 Best 50", "divingfish");

        await bestsImage.WriteToResponseAsync(responseStream);
    }

    public override async Task GetAnimeFromDivingFish(DivingFishBestsRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfDataAsync(request.Qq, request.Frame, request.Plate, request.Icon);
        using Image bestsImage = await new Drawer().DrawBestsAsync(user, bestEver, bestCurrent, everTotal, currentTotal,
            "水鱼 Best 50", "divingfish", true);

        await bestsImage.WriteToResponseAsync(responseStream, true);
    }

    public override async Task GetFromDivingFishWithLevelSeg(DivingFishBestsRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareDfDataAsync(request.Qq, request.Frame, request.Plate, request.Icon);
        using Image bestsImage = await new Drawer().DrawBestsAsync(user, bestEver, bestCurrent, everTotal, currentTotal,
            "水鱼 Best 50", "divingfish", false, true);

        await bestsImage.WriteToResponseAsync(responseStream, true);
    }
}