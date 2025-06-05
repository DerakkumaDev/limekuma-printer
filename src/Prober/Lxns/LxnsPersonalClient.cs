using Limekuma.Prober.Lxns.Models;

namespace Limekuma.Prober.Lxns;

public class LxnsPersonalClient : LxnsDataClient
{
    public LxnsPersonalClient(string userToken) : base()
    {
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-User-Token", userToken);
    }

    public async Task<Player> GetPlayerAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<Player>("/api/v0/user/maimai/player", cancellationToken);

    public async Task<List<Record>> GetRecordsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<Record>>("/api/v0/user/maimai/player/scores", cancellationToken);

    public async Task UploadRecordsAsync(IEnumerable<Record> records, CancellationToken cancellationToken = default) =>
        await PostAsync("/api/v0/user/maimai/player/scores", records, cancellationToken);
}
