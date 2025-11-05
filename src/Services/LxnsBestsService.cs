using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using Limekuma.Render;
using Limekuma.Utils;
using LimeKuma;
using SixLabors.ImageSharp;
using System.Net;

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
            try
            {
                player = await lxnsDev.GetPlayerByQQAsync(qq.Value);
            }
            catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
            {
                throw new RpcException(new(StatusCode.NotFound, ex.Message, ex));
            }
            catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden)
            {
                throw new RpcException(new(StatusCode.PermissionDenied, ex.Message, ex));
            }
        }
        else if (!string.IsNullOrEmpty(personalToken))
        {
            LxnsPersonalClient lxns = new(personalToken);
            try
            {
                player = await lxns.GetPlayerAsync();
            }
            catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized)
            {
                throw new RpcException(new(StatusCode.Unauthenticated, ex.Message, ex));
            }

            player.Client = lxnsDev;
        }
        else
        {
            throw new RpcException(new(StatusCode.InvalidArgument, "QQ or token is required."));
        }

        Bests bests;
        try
        {
            bests = await player.GetBestsAsync();
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

        List<CommonRecord> bestEver = bests.Ever.ConvertAll<CommonRecord>(_ => _);
        bestEver.SortRecordForBests();

        List<CommonRecord> bestCurrent = bests.Current.ConvertAll<CommonRecord>(_ => _);
        bestCurrent.SortRecordForBests();

        await PrepareDataAsync(user, bestEver, bestCurrent);

        return (user, bestEver, bestCurrent, bests.EverTotal, bests.CurrentTotal);
    }

    public override async Task GetFromLxns(LxnsBestsRequest request, IServerStreamWriter<ImageResponse> responseStream,
        ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareLxnsDataAsync(request.DevToken, request.Qq, request.PersonalToken);
        using Image bestsImage =
            await new Drawer().DrawBestsAsync(user, bestEver, bestCurrent, everTotal, currentTotal, "落雪 Best 50",
                "lxns");

        await responseStream.WriteToResponseAsync(bestsImage);
    }

    public override async Task GetAnimeFromLxns(LxnsBestsRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareLxnsDataAsync(request.DevToken, request.Qq, request.PersonalToken);
        using Image bestsImage = await new Drawer().DrawBestsAsync(user, bestEver, bestCurrent, everTotal, currentTotal,
            "落雪 Best 50", "lxns", true);

        await responseStream.WriteToResponseAsync(bestsImage, true);
    }

    public override async Task GetFromLxnsWithLevelSeg(LxnsBestsRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        (CommonUser user, List<CommonRecord> bestEver, List<CommonRecord> bestCurrent, int everTotal,
            int currentTotal) = await PrepareLxnsDataAsync(request.DevToken, request.Qq, request.PersonalToken);
        using Image bestsImage = await new Drawer().DrawBestsAsync(user, bestEver, bestCurrent, everTotal, currentTotal,
            "落雪 Best 50", "lxns", false, true);

        await responseStream.WriteToResponseAsync(bestsImage, true);
    }
}