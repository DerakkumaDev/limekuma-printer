using Grpc.Core;
using Limekuma.Prober.Common;
using Limekuma.Prober.DivingFish.Enums;
using Limekuma.Prober.DivingFish.Models;
using Limekuma.Render;
using Limekuma.Utils;
using SixLabors.ImageSharp;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Limekuma.Services;

public partial class BestsService
{
    private static async Task<(CommonUser, ImmutableArray<CommonRecord>)> PrepareDfRecordsForProcessAsync(string token,
        uint? qq, int? frame, int? plate, int? icon)
    {
        ServiceExecutionHelper.EnsureArgument(qq.HasValue && frame.HasValue && plate.HasValue && icon.HasValue);
        PlayerData player = await DfGatewayService.GetPlayerDataAsync(token, qq!.Value);

        CommonUser user = player;
        user.FrameId = frame!.Value;
        user.PlateId = plate!.Value;
        user.IconId = icon!.Value;
        return (user, [.. player.Records.Select(x => (CommonRecord)x)]);
    }

    private static async Task<(CommonUser, ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>, int, int)>
        PrepareDfDataAsync(uint? qq, int? frame, int? plate, int? icon)
    {
        ServiceExecutionHelper.EnsureArgument(qq.HasValue && frame.HasValue && plate.HasValue && icon.HasValue);
        Player player = await DfGatewayService.GetPlayerAsync(qq!.Value);

        CommonUser user = player;
        user.FrameId = frame!.Value;
        user.PlateId = plate!.Value;
        user.IconId = icon!.Value;

        ImmutableArray<CommonRecord> bestEver =
            [.. player.Bests.Ever.Select(x => (CommonRecord)x).SortRecordForBests()];
        int everTotal = bestEver.Sum(x => x.DXRating);

        ImmutableArray<CommonRecord> bestCurrent =
            [.. player.Bests.Current.Select(x => (CommonRecord)x).SortRecordForBests()];
        int currentTotal = bestCurrent.Sum(x => x.DXRating);

        await PrepareDataAsync(user, bestEver, bestCurrent);

        return (user, bestEver, bestCurrent, everTotal, currentTotal);
    }

    private static async Task<(CommonUser, ImmutableArray<CommonRecord>, ImmutableArray<CommonRecord>, int, int)>
        PrepareRiRenDfDataAsync()
    {
        CommonRecord[] allRecords = Songs.Shared.AsParallel().SelectMany(song =>
        {
            if (!int.TryParse(song.Id, out int id))
            {
                return [];
            }

            int chartCount = Math.Min(song.Charts.Count, Math.Min(song.LevelValues.Count, song.Levels.Count));
            return Enumerable.Range(0, chartCount).Select(i => (CommonRecord)new Record
            {
                Achievements = 101,
                ComboFlag = ComboFlags.AllPerfectPlus,
                Difficulty = song.Type is SongTypes.Utage ? Difficulties.Utage : (Difficulties)(i + 1),
                DifficultyIndex = i,
                DXRating = (int)(song.LevelValues[i] * 22.512),
                DXScore = song.Charts[i].Notes.Total * 3,
                Id = id,
                Level = song.Levels[i],
                LevelValue = song.LevelValues[i],
                Rank = Ranks.SSSPlus,
                SyncFlag = SyncFlags.FullSyncDXPlus,
                Title = song.Title,
                Type = song.Type
            });
        }).ToArray();
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

    public override async Task GetFromDivingFish(DivingFishBestsRequest request,
        IServerStreamWriter<ImageResponse> responseStream, ServerCallContext context)
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
            (user, ImmutableArray<CommonRecord> records) = await PrepareDfRecordsForProcessAsync(request.Token,
                request.Qq, request.Frame, request.Plate, request.Icon);
            (bestEver, bestCurrent, everTotal, currentTotal, user2p) = await ProcessBestsByTagsAsync(requestTags,
                request.Condition, records, async condition =>
                {
                    CoopExtraInfo extraInfo =
                        ServiceExecutionHelper.DeserializeOrThrow<CoopExtraInfo>(condition, "Invalid arguments");
                    return await PrepareDfRecordsForProcessAsync(request.Token, extraInfo.Qq, extraInfo.Frame,
                        extraInfo.Plate, extraInfo.Icon);
                });
        }
        else if (requestTags.Contains("common"))
        {
            (user, bestEver, bestCurrent, everTotal, currentTotal) =
                await PrepareDfDataAsync(request.Qq, request.Frame, request.Plate, request.Icon);
        }
        else if (requestTags.Contains("riren"))
        {
            (user, bestEver, bestCurrent, everTotal, currentTotal) = await PrepareRiRenDfDataAsync();
        }
        else
        {
            throw new RpcException(new(StatusCode.InvalidArgument, "Invalid arguments"));
        }

        using Image bestsImage = await new Drawer().DrawBestsAsync(user, bestEver, bestCurrent, everTotal, currentTotal,
            request.Condition, "divingfish", requestTags, user2p);

        await responseStream.WriteToResponseAsync(bestsImage);
    }

    public record CoopExtraInfo(
        [property: JsonPropertyName("qq")]
        uint Qq,
        [property: JsonPropertyName("frame")]
        int Frame,
        [property: JsonPropertyName("plate")]
        int Plate,
        [property: JsonPropertyName("icon")]
        int Icon);
}
