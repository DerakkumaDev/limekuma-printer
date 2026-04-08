using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Render;
using Limekuma.Utils;
using SixLabors.ImageSharp;
using System.Collections.Immutable;
using System.Net;

namespace Limekuma.Services;

public partial class ListService
{
    public override async Task GetFromDivingFish(DivingFishListRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        DfDeveloperClient df = new(request.Token);
        PlayerData player;
        try
        {
            player = await df.GetPlayerDataAsync(request.Qq);
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
        user.PlateId = request.Plate;
        user.IconId = request.Icon;

        (Func<CommonRecord, bool> predicate, bool maskMutex) = ScoreFilterHelper.GetPredicateByTags(request.Tags, request.Condition);
        bool mayMask = player.Records.Any(r => r.DXScore is 0 && (r.DXStar > 0 || r.Rank > Ranks.A));
        if (mayMask && maskMutex)
        {
            throw new RpcException(new(StatusCode.PermissionDenied, "Mask enabled"));
        }

        ImmutableArray<CommonRecord> records = [.. player.Records.ConvertAll<CommonRecord>(_ => _).Where(predicate).SortRecordForList()];
        (ImmutableArray<int> counts, int startIndex, int endIndex) = await PrepareDataAsync(user, records, request.Page);

        using Image listImage = await new Drawer().DrawListAsync(user, records[startIndex..endIndex], request.Page, counts,
            records.Length, startIndex, request.Condition, mayMask, "divingfish", request.Tags);

        await responseStream.WriteToResponseAsync(listImage);
    }
}