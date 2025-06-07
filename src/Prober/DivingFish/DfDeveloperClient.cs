using Limekuma.Prober.DivingFish.Models;

namespace Limekuma.Prober.DivingFish;

public class DfDeveloperClient(string token) : DfDataClient("Developer-Token", token)
{
    public async Task<PlayerData> GetPlayerDataAsync(uint qq, CancellationToken cancellationToken = default) =>
        await GetAsync<PlayerData>($"/api/maimaidxprober/dev/player/records?qq={qq}", cancellationToken);

    public async Task<PlayerData> GetPlayerDataAsync(string account, CancellationToken cancellationToken = default) =>
        await GetAsync<PlayerData>($"/api/maimaidxprober/dev/player/records?username={account}", cancellationToken);

    public async Task<Dictionary<string, List<Record>>> GetRecordsAsync(uint qq, int id,
        CancellationToken cancellationToken = default) =>
        await PostAsync<dynamic, Dictionary<string, List<Record>>>("/api/maimaidxprober/dev/player/record",
            new { qq, music_id = id }, cancellationToken);

    public async Task<Dictionary<string, List<Record>>> GetRecordsAsync(string account, int id,
        CancellationToken cancellationToken = default) =>
        await PostAsync<dynamic, Dictionary<string, List<Record>>>("/api/maimaidxprober/dev/player/record",
            new { username = account, music_id = id }, cancellationToken);

    public async Task<Dictionary<string, List<Record>>> GetRecordsAsync(uint qq, IEnumerable<int> id,
        CancellationToken cancellationToken = default) =>
        await PostAsync<dynamic, Dictionary<string, List<Record>>>("/api/maimaidxprober/dev/player/record",
            new { qq, music_id = id }, cancellationToken);

    public async Task<Dictionary<string, List<Record>>> GetRecordsAsync(string account, IEnumerable<int> id,
        CancellationToken cancellationToken = default) =>
        await PostAsync<dynamic, Dictionary<string, List<Record>>>("/api/maimaidxprober/dev/player/record",
            new { username = account, music_id = id }, cancellationToken);
}