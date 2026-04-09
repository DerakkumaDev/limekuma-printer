using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Render;
using Limekuma.Utils;
using SixLabors.ImageSharp;
using System.Collections.Immutable;

namespace Limekuma.Services;

public partial class ListService
{
    public override async Task GetFromLxns(LxnsListRequest request, IServerStreamWriter<ImageResponse> responseStream,
        ServerCallContext context)
    {
        CommonUser player = await LxnsGatewayService.GetPlayerByPersonalTokenAsync(request.DevToken, request.PersonalToken);
        List<CommonRecord> sourceRecords = [.. (await LxnsGatewayService.GetRecordsAsync(request.PersonalToken)).ConvertAll<CommonRecord>(_ => _)];
        (ImmutableArray<CommonRecord> cRecords, bool mayMask) = BuildListRecords(request.Tags, request.Condition, sourceRecords);

        (ImmutableArray<int> counts, int startIndex, int endIndex) = await PrepareDataAsync(player, cRecords, request.Page);

        using Image listImage = await new Drawer().DrawListAsync(player, cRecords[startIndex..endIndex], request.Page, counts,
            cRecords.Length, startIndex, request.Condition, mayMask, "lxns", request.Tags);

        await responseStream.WriteToResponseAsync(listImage);
    }
}
