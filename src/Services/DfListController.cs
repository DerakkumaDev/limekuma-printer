using Grpc.Core;
using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Utils;
using SixLabors.ImageSharp;

namespace Limekuma.Services;

public partial class ListService
{
    public override async Task<ImageReply> GetFromDivingFish(DivingFishListRequest request, ServerCallContext context)
    {
        DfDeveloperClient df = new(request.Token);
        PlayerData player = await df.GetPlayerDataAsync(request.Qq);
        CommonUser user = player;
        user.PlateId = request.Plate;
        user.IconId = request.Icon;

        List<Record> records = [.. player.Records.Where(x => x.Level == request.Level)];
        int count = records.Count;
        List<CommonRecord> cRecords = records.ConvertAll<CommonRecord>(_ => _);
        cRecords.SortRecordForList();
        (int[] counts, int startIndex, int endIndex) = await PrepareDataAsync(user, cRecords, request.Page);
        int total = (int)Math.Ceiling((double)count / 55);

        using Image listImage =
            new ListDrawer().Draw(user, cRecords[startIndex..endIndex], request.Page, total, counts, request.Level);

        return new()
        {
            Image = await ServiceHelper.ReturnImageAsync(listImage)
        };

    }
}