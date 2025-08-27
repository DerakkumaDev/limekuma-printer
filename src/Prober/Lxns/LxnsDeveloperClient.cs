using Limekuma.Prober.Lxns.Enums;
using Limekuma.Prober.Lxns.Models;

namespace Limekuma.Prober.Lxns;

public class LxnsDeveloperClient : LxnsDataClient
{
    public LxnsDeveloperClient(string token) =>
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

    public async Task CreateOrUpdateAsync(Player player, CancellationToken cancellationToken = default) =>
        await PostAsync("/api/v0/maimai/player", player, cancellationToken);

    public async Task<Player> GetPlayerAsync(long friendCode, CancellationToken cancellationToken = default)
    {
        Player player = await GetAsync<Player>($"/api/v0/maimai/player/{friendCode}", cancellationToken);
        player.Client = this;
        return player;
    }

    public async Task<Player> GetPlayerByQQAsync(uint qq, CancellationToken cancellationToken = default)
    {
        Player player = await GetAsync<Player>($"/api/v0/maimai/player/qq/{qq}", cancellationToken);
        player.Client = this;
        return player;
    }

    public async Task<Record> GetBestAsync(long friendCode, int id, Difficulties difficulty, SongTypes type,
        CancellationToken cancellationToken = default) =>
        await GetAsync<Record>(
            $"/api/v0/maimai/player/{friendCode}/best?song_id={id}&level_index={(int)difficulty}&song_type={type.ToString().ToLower()}",
            cancellationToken);

    public async Task<Record> GetBestAsync(long friendCode, string title, Difficulties difficulty, SongTypes type,
        CancellationToken cancellationToken = default) =>
        await GetAsync<Record>(
            $"/api/v0/maimai/player/{friendCode}/best?song_name={title}&level_index={(int)difficulty}&song_type={type.ToString().ToLower()}",
            cancellationToken);

    public async Task<Bests> GetBestsAsync(long friendCode, CancellationToken cancellationToken = default) =>
        await GetAsync<Bests>($"/api/v0/maimai/player/{friendCode}/bests", cancellationToken);

    public async Task<Bests> GetAllPerfectBestsAsync(long friendCode, CancellationToken cancellationToken = default) =>
        await GetAsync<Bests>($"/api/v0/maimai/player/{friendCode}/bests/ap", cancellationToken);

    public async Task<List<Record>> GetRecordsAsync(long friendCode, int id, SongTypes type,
        CancellationToken cancellationToken = default) =>
        await GetAsync<List<Record>>(
            $"/api/v0/maimai/player/{friendCode}/bests?song_id={id}&song_type={type.ToString().ToLower()}",
            cancellationToken);

    public async Task<List<Record>> GetRecordsAsync(long friendCode, string title, SongTypes type,
        CancellationToken cancellationToken = default) =>
        await GetAsync<List<Record>>(
            $"/api/v0/maimai/player/{friendCode}/bests?song_name={title}&song_type={type.ToString().ToLower()}",
            cancellationToken);

    public async Task UploadRecordsAsync(long friendCode, IEnumerable<Record> records,
        CancellationToken cancellationToken = default) =>
        await PostAsync($"/api/v0/maimai/player/{friendCode}/scores", new { records }, cancellationToken);

    public async Task<List<Record>> GetRecentsAsync(long friendCode, CancellationToken cancellationToken = default) =>
        await GetAsync<List<Record>>($"/api/v0/maimai/player/{friendCode}/recents", cancellationToken);

    public async Task<List<SimpleRecord>> GetAllRecordsAsync(long friendCode,
        CancellationToken cancellationToken = default) =>
        await GetAsync<List<SimpleRecord>>($"/api/v0/maimai/player/{friendCode}/scores", cancellationToken);

    public async Task<Dictionary<string, int>> GetHeatmapAsync(long friendCode,
        CancellationToken cancellationToken = default) =>
        await GetAsync<Dictionary<string, int>>($"/api/v0/maimai/player/{friendCode}/heatmap", cancellationToken);

    public async Task<List<RatingTrend>> GetDXRatingTrendAsync(long friendCode, int? version = null,
        CancellationToken cancellationToken = default)
    {
        string url = $"/api/v0/maimai/player/{friendCode}/trend";
        if (version.HasValue)
        {
            url += $"?version={version}";
        }

        return await GetAsync<List<RatingTrend>>(url, cancellationToken);
    }

    public async Task<List<Record>> GetHistoryAsync(long friendCode, int id, SongTypes type, Difficulties difficulty,
        CancellationToken cancellationToken = default) =>
        await GetAsync<List<Record>>(
            $"/api/v0/maimai/player/{friendCode}/score/history?song_id={id}&song_type={type.ToString().ToLower()}&level_index={(int)difficulty}",
            cancellationToken);

    public async Task<NamePlate> GetNamePlateProgressAsync(long friendCode, int id,
        CancellationToken cancellationToken = default) =>
        await GetAsync<NamePlate>($"/api/v0/maimai/player/{friendCode}/plate/{id}", cancellationToken);

    public async Task UploadFromHtmlAsync(long friendCode, string html,
        CancellationToken cancellationToken = default) =>
        await PostAsync($"/api/v0/maimai/player/{friendCode}/html", html, cancellationToken);
}