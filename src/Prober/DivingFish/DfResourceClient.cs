using Limekuma.Prober.DivingFish.Models;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.DivingFish;

public class DfResourceClient : DfClient
{
    public async Task<List<Song>> GetSongListAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<Song>>("/api/maimaidxprober/music_data", cancellationToken);

    public async Task<PlayerInfo> GetPlayerAsync(uint qq, CancellationToken cancellationToken = default) =>
        await PostAsync<dynamic, PlayerInfo>("/api/maimaidxprober/query/player", new { qq = qq.ToString(), b50 = true },
            cancellationToken);

    public async Task<PlayerInfo> GetPlayerAsync(uint qq, bool best50, CancellationToken cancellationToken = default) =>
        await PostAsync<dynamic, PlayerInfo>("/api/maimaidxprober/query/player",
            new { qq = qq.ToString(), b50 = best50 }, cancellationToken);

    public async Task<PlayerInfo> GetPlayerAsync(string name, CancellationToken cancellationToken = default) =>
        await PostAsync<dynamic, PlayerInfo>("/api/maimaidxprober/query/player", new { username = name, b50 = true },
            cancellationToken);

    public async Task<PlayerInfo> GetPlayerAsync(string name, bool best50,
        CancellationToken cancellationToken = default) =>
        await PostAsync<dynamic, PlayerInfo>("/api/maimaidxprober/query/player", new { username = name, b50 = best50 },
            cancellationToken);

    public async Task<List<SimpleRecord>> GetRecordsInVersionAsync(uint qq, IEnumerable<string> versions,
        CancellationToken cancellationToken = default)
    {
        RecordsInVersionResponse response = await PostAsync<dynamic, RecordsInVersionResponse>(
            "/api/maimaidxprober/query/plate", new { qq = qq.ToString(), version = versions }, cancellationToken);
        return response.VerList;
    }

    public async Task<List<SimpleRecord>> GetRecordsInVersionAsync(string name, IEnumerable<string> versions,
        CancellationToken cancellationToken = default)
    {
        RecordsInVersionResponse response = await PostAsync<dynamic, RecordsInVersionResponse>(
            "/api/maimaidxprober/query/plate", new { username = name, version = versions }, cancellationToken);
        return response.VerList;
    }

    public async Task<Dictionary<int, double>> GetHotSongListAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<Dictionary<int, double>>("/api/maimaidxprober/hot_music", cancellationToken);

    public async Task<Dictionary<int, VoteResult>> GetVoteResultAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<Dictionary<int, VoteResult>>("/api/maimaidxprober/vote_result", cancellationToken);

    public async Task<List<SimplePlayer>> GetRatingRankingAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<SimplePlayer>>("/api/maimaidxprober/rating_ranking", cancellationToken);

    public async Task<Status> GetChartStstusAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<Status>("/api/maimaidxprober/chart_stats", cancellationToken);

    private record RecordsInVersionResponse
    {
        [JsonPropertyName("verlist")]
        public required List<SimpleRecord> VerList { get; set; }
    }
}