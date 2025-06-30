using Grpc.Core;
using Limekuma.Draw;
using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using Limekuma.Utils;
using SixLabors.ImageSharp;

namespace Limekuma.Services;

public partial class ListService
{
    public async Task<ImageReply> GetLxnsListAsync(LxnsListRequest request, ServerCallContext context)
    {
        LxnsPersonalClient lxns = new(request.PersonalToken);
        List<Record> records = await lxns.GetRecordsAsync();
        CommonUser user = await lxns.GetPlayerAsync();

        records = [.. records.Where(x => x.Level == request.Level)];
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