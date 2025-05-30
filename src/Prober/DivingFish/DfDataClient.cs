using DXKumaBot.Backend.Utils;

namespace DXKumaBot.Backend.Prober.DivingFish;

public abstract class DfDataClient : DfClient
{
    protected DfDataClient(string name, string token) : base()
    {
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(name, token);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response;
    }
}