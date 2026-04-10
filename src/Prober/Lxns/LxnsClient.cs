using System.Reflection;
using System.Text.Json;

namespace Limekuma.Prober.Lxns;

public abstract class LxnsClient
{
    private static readonly HttpClientHandler SharedHandler = new();
    private static readonly JsonSerializerOptions SharedJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private static readonly string? UserAgentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    protected HttpClient _httpClient;
    protected JsonSerializerOptions _jsonOptions;

    protected LxnsClient()
    {
        _httpClient = new(SharedHandler, false)
        {
            BaseAddress = new("https://maimai.lxns.net/")
        };
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new("limekuma", UserAgentVersion));
        _jsonOptions = SharedJsonOptions;
    }

    protected async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<T>(path, _jsonOptions, cancellationToken) ??
        throw new InvalidOperationException("Failed to deserialize response");

    protected async Task<HttpResponseMessage> PostAsync<T>(string path, T value,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(path, value, _jsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        return response;
    }
}
