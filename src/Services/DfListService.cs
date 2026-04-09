using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Render;
using Limekuma.Utils;
using SixLabors.ImageSharp;
using System.Collections.Immutable;

namespace Limekuma.Services;

public partial class ListService
{
    public override async Task GetFromDivingFish(DivingFishListRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
    {
        PlayerData player = await DfGatewayService.GetPlayerDataAsync(request.Token, request.Qq);

        CommonUser user = player;
        user.PlateId = request.Plate;
        user.IconId = request.Icon;

        (ImmutableArray<CommonRecord> records, bool mayMask) = BuildListRecords(request.Tags, request.Condition,
            player.Records.ConvertAll<CommonRecord>(_ => _));

        (ImmutableArray<int> counts, int startIndex, int endIndex) =
            await PrepareDataAsync(user, records, request.Page);

        using Image listImage = await new Drawer().DrawListAsync(user, records[startIndex..endIndex], request.Page,
            counts, records.Length, startIndex, request.Condition, mayMask, "divingfish", request.Tags);

        await responseStream.WriteToResponseAsync(listImage);
    }
}
