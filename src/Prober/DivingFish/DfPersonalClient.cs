using Limekuma.Prober.DivingFish.Models;

namespace Limekuma.Prober.DivingFish;

public class DfPersonalClient(string personalToken) : DfDataClient("Import-Token", personalToken)
{
    public async Task<PlayerData> GetPlayerDataAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<PlayerData>("/api/maimaidxprober/player/records", cancellationToken);

    public async Task UploadRecordsAsync(IEnumerable<Record> records, CancellationToken cancellationToken = default) =>
        await PostAsync("/api/maimaidxprober/player/update_records", records, cancellationToken);

    public async Task UploadRecordAsync(Record record, CancellationToken cancellationToken = default) =>
        await PostAsync("/api/maimaidxprober/player/update_record", record, cancellationToken);

    public async Task DeleteAllRecordsAsync(CancellationToken cancellationToken = default) =>
        await DeleteAsync("/api/maimaidxprober/player/delete_records", cancellationToken);
}