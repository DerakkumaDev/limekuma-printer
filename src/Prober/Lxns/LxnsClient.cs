using System.Reflection;
using System.Text.Json;

namespace Limekuma.Prober.Lxns;

public abstract class LxnsClient
{
    protected HttpClient _httpClient;
    protected JsonSerializerOptions _jsonOptions;

    protected LxnsClient()
    {
        _httpClient = new()
        {
            BaseAddress = new("https://maimai.lxns.net/")
        };
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new("limekuma",
            Assembly.GetExecutingAssembly().GetName().Version?.ToString()));

        _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    protected async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<T>(path, _jsonOptions, cancellationToken)
        ?? throw new InvalidOperationException("Failed to deserialize response");

    protected async Task<HttpResponseMessage> PostAsync<T>(string path, T value,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(path, value, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response;
    }
}