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

        List<Record> records = [.. player.Records.Where(x => x.Level == request.Level)];
        int count = records.Count;
        List<CommonRecord> cRecords = records.ConvertAll<CommonRecord>(_ => _);
        cRecords.SortRecordForList();
        (int[] counts, int startIndex, int endIndex) = await PrepareDataAsync(user, cRecords, request.Page);
        int total = (int)Math.Ceiling((double)count / 55);

        using Image listImage = await new Drawer().DrawListAsync(user, cRecords[startIndex..endIndex], request.Page,
            total, counts, request.Level, "divingfish");

        await responseStream.WriteToResponseAsync(listImage);
    }
}