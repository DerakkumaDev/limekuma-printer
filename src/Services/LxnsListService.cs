using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using Limekuma.Render;
using Limekuma.ScoreFilter;
using Limekuma.Utils;
using SixLabors.ImageSharp;
using System.Collections.Immutable;
using System.Net;

namespace Limekuma.Services;

public partial class ListService
{
    public override async Task GetFromLxns(LxnsListRequest request, IServerStreamWriter<ImageResponse> responseStream,
        ServerCallContext context)
    {
        Player player;
        LxnsDeveloperClient lxnsDev = new(request.DevToken);
        LxnsPersonalClient lxns = new(request.PersonalToken);
        try
        {
            player = await lxns.GetPlayerAsync(lxnsDev);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized)
        {
            throw new RpcException(new(StatusCode.Unauthenticated, ex.Message, ex));
        }

        try
        {
            player = await lxnsDev.GetPlayerAsync(player.FriendCode);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound)
        {
            throw new RpcException(new(StatusCode.NotFound, ex.Message, ex));
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden)
        {
            throw new RpcException(new(StatusCode.PermissionDenied, ex.Message, ex));
        }

        List<Record> records;
        try
        {
            records = await lxns.GetRecordsAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized)
        {
            throw new RpcException(new(StatusCode.Unauthenticated, ex.Message, ex));
        }

        IScoreFilter filter = ScoreFilterHelper.GetFilterByTags(request.Tags) ?? throw new RpcException(new(StatusCode.InvalidArgument, "Invalid filter tags"));
        bool mayMask = records.Any(r => r.DXScore is 0 && (r.DXStar > 0 || r.Rank > Ranks.A));
        if (mayMask && filter.MaskMutex)
        {
            throw new RpcException(new(StatusCode.PermissionDenied, "Mask enabled"));
        }

        ImmutableArray<CommonRecord> cRecords = [.. records.ConvertAll<CommonRecord>(_ => _).Where(filter.GetFilter(request.Condition)).SortRecordForList()];
        (ImmutableArray<int> counts, int startIndex, int endIndex) = await PrepareDataAsync(player, cRecords, request.Page);

        using Image listImage = await new Drawer().DrawListAsync(player, cRecords[startIndex..endIndex], request.Page, counts,
            cRecords.Length, startIndex, request.Condition, mayMask, "lxns", request.Tags);

        await responseStream.WriteToResponseAsync(listImage);
    }
}