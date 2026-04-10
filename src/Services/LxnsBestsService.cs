using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using Limekuma.Render;
using Limekuma.Utils;
using SixLabors.ImageSharp;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Limekuma.Services;

public partial class BestsService
{
    private static async Task<(CommonUser, ImmutableArray<CommonRecord>)> PrepareLxnsRecordsForProcessAsync(
        string devToken, string personalToken)
    {
        Task<Player> playerTask = LxnsGatewayService.GetPlayerByPersonalTokenAsync(devToken, personalToken);
        Task<List<Record>> recordsTask = LxnsGatewayService.GetRecordsAsync(personalToken);
        await Task.WhenAll(playerTask, recordsTask);

        Player player = await playerTask;
        List<Record> records = await recordsTask;
        return (player, [.. records.Select(x => (CommonRecord)x)]);
    }

    private static async Task<(CommonUser, ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>, int, int)>
        PrepareLxnsDataAsync(string devToken, uint? qq, string? personalToken)
    {
        Player player;
        if (qq.HasValue)
        {
            player = await LxnsGatewayService.GetPlayerByQqAsync(devToken, qq.Value);
        }
        else if (!string.IsNullOrEmpty(personalToken))
        {
            player = await LxnsGatewayService.GetPlayerByPersonalTokenAsync(devToken, personalToken);
        }
        else
        {
            throw new RpcException(new(StatusCode.InvalidArgument, "QQ or token is required."));
        }

        Bests bests = await LxnsGatewayService.GetBestsAsync(player);

        CommonUser user = player;

        ImmutableArray<CommonRecord> bestEver = [.. bests.Ever.Select(x => (CommonRecord)x).SortRecordForBests()];
        ImmutableArray<CommonRecord> bestCurrent =
            [.. bests.Current.Select(x => (CommonRecord)x).SortRecordForBests()];
        await PrepareDataAsync(user, bestEver, bestCurrent);

        return (user, bestEver, bestCurrent, bests.EverTotal, bests.CurrentTotal);
    }

    private static async Task<(CommonUser, ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>, int, int)>
        PrepareRiRenLxnsDataAsync()
    {
        LxnsResourceClient resource = new();
        SongData songData = await resource.GetSongsAsync(includeNotes: true);
        CommonRecord[] allRecords = songData.Songs.AsParallel().SelectMany(song => song.Charts.Standard
            .Concat(song.Charts.DX).Where(chart => chart.Notes is not null).Select(chart => (CommonRecord)new Record
            {
                Achievements = 101,
                Difficulty = chart.Difficulty,
                Id = song.Id,
                DXScore = chart.Notes!.Total * 3,
                DXScoreRank = 5,
                Level = chart.Level,
                Title = song.Title,
                Type = chart.Type,
                ComboFlag = ComboFlags.AllPerfectPlus,
                DXRating = (int)(chart.LevelValue * 22.512),
                Rank = Ranks.SSSPlus,
                SyncFlag = SyncFlags.FullSyncDXPlus
            })).ToArray();
        (ImmutableArray<CommonRecord> bestEver, ImmutableArray<CommonRecord> bestCurrent) =
            allRecords.SplitTopBestsByQuota(35, 15);
        int everTotal = bestEver.Sum(x => x.DXRating);
        int currentTotal = bestCurrent.Sum(x => x.DXRating);
        CommonUser user = new()
        {
            Name = "ＤＸＫｕｍａ",
            Rating = everTotal + currentTotal,
            TrophyColor = TrophyColor.Rainbow,
            TrophyText = "でらっくま",
            CourseRank = CommonCourseRank.Urakaiden,
            ClassRank = ClassRank.LEGEND,
            IconId = 1,
            PlateId = 1,
            FrameId = 1
        };

        await PrepareDataAsync(user, bestEver, bestCurrent);
        return (user, bestEver, bestCurrent, everTotal, currentTotal);
    }

    public override async Task GetFromLxns(LxnsBestsRequest request, IServerStreamWriter<ImageResponse> responseStream,
        ServerCallContext context)
    {
        FrozenSet<string> requestTags = request.Tags.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        CommonUser user;
        CommonUser? user2p = null;
        ImmutableArray<CommonRecord> bestEver;
        ImmutableArray<CommonRecord> bestCurrent;
        int everTotal;
        int currentTotal;
        if (ScoreProcesserHelper.GetProcesserByTags(requestTags) is not null)
        {
            (user, ImmutableArray<CommonRecord> records) =
                await PrepareLxnsRecordsForProcessAsync(request.DevToken, request.PersonalToken);
            (bestEver, bestCurrent, everTotal, currentTotal, user2p) = await ProcessBestsByTagsAsync(requestTags,
                request.Condition, records,
                async condition => await PrepareLxnsRecordsForProcessAsync(request.DevToken, condition));
        }
        else if (requestTags.Contains("common"))
        {
            (user, bestEver, bestCurrent, everTotal, currentTotal) =
                await PrepareLxnsDataAsync(request.DevToken, request.Qq, request.PersonalToken);
        }
        else if (requestTags.Contains("riren"))
        {
            (user, bestEver, bestCurrent, everTotal, currentTotal) = await PrepareRiRenLxnsDataAsync();
        }
        else
        {
            throw new RpcException(new(StatusCode.InvalidArgument, "Invalid arguments"));
        }

        using Image bestsImage = await new Drawer().DrawBestsAsync(user, bestEver, bestCurrent, everTotal, currentTotal,
            request.Condition, "lxns", requestTags);

        await responseStream.WriteToResponseAsync(bestsImage);
    }
}
