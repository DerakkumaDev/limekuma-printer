using DXKumaBot.Backend.Utils;
using System.Text.Json;

namespace DXKumaBot.Backend.Prober.Lxns;

public abstract class LxnsClient
{
    protected HttpClient _httpClient;
    protected JsonSerializerOptions _jsonOptions;

    protected LxnsClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://maimai.lxns.net/")
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _jsonOptions.Converters.Add(new OptionalTATBConverter());
    }

    protected async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<T>(path, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response");

    protected async Task<HttpResponseMessage> PostAsync<T>(string path, T value, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(path, value, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response;
    }
}
