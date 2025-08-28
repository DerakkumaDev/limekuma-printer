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
        LxnsPersonalClient lxns = new(request.PersonalToken);
        CommonUser user;
        try
        {
            user = await lxns.GetPlayerAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message, ex));
        }

        List<Record> records;
        try
        {
            records = await lxns.GetRecordsAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message, ex));
        }

        records = [.. records.Where(x => x.Level == request.Level)];
        int count = records.Count;
        List<CommonRecord> cRecords = records.ConvertAll<CommonRecord>(_ => _);
        cRecords.SortRecordForList();
        (int[] counts, int startIndex, int endIndex) = await PrepareDataAsync(user, cRecords, request.Page);
        int total = (int)Math.Ceiling((double)count / 55);

        using Image listImage = new ListDrawer().Draw(user, cRecords[startIndex..endIndex], request.Page, total, counts,
            request.Level);

        await listImage.WriteToResponseAsync(responseStream);
    }
}