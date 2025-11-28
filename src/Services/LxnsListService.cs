using Grpc.Core;
using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using Limekuma.Utils;
using LimeKuma;
using SixLabors.ImageSharp;
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

        records = [.. records.Where(x => x.Level == request.Level)];
        int count = records.Count;
        List<CommonRecord> cRecords = records.ConvertAll<CommonRecord>(_ => _);
        cRecords.SortRecordForList();
        (int[] counts, int startIndex, int endIndex) = await PrepareDataAsync(player, cRecords, request.Page);
        int total = (int)Math.Ceiling((double)count / 55);

        using Image listImage = new ListDrawer().Draw(player, cRecords[startIndex..endIndex], request.Page, total, counts, request.Level, "lxns");

        await listImage.WriteToResponseAsync(responseStream);
    }
}