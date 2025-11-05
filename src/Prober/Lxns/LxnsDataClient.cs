using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns;

public abstract class LxnsDataClient : LxnsClient
{
    protected new async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        Response<T> response = await base.GetAsync<Response<T>>(path, cancellationToken);
        if (!response.Success || response.Code is not 200 || response.Data is null)
        {
            throw new InvalidOperationException($"{response.Code}: {response.Message}");
        }

        return response.Data;
    }

    protected new async Task PostAsync<T>(string path, T value, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage responseMessage = await base.PostAsync(path, value, cancellationToken);
        Response response = await responseMessage.Content.ReadFromJsonAsync<Response>(_jsonOptions, cancellationToken)
                            ?? throw new InvalidOperationException("Failed to deserialize response");
        if (!response.Success || response.Code is not 200)
        {
            throw new InvalidOperationException($"{response.Code}: {response.Message}");
        }
    }

    private record Response
    {
        [JsonPropertyName("success")]
        public required bool Success { get; set; }

        [JsonPropertyName("code")]
        public required int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    private record Response<T> : Response
    {
        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}