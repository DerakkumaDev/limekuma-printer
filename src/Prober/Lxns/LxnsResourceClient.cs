using Limekuma.Prober.Lxns.Models;
using System.Text.Json.Serialization;

namespace Limekuma.Prober.Lxns;

public class LxnsResourceClient : LxnsClient
{
    private async Task<T> GetListAsync<T>(string type, string includeType, int? version = null, bool? include = null, CancellationToken cancellationToken = default)
    {
        string url = $"/api/v0/maimai/{type}/list";
        List<string> queryParams = [];
        if (version.HasValue)
        {
            queryParams.Add($"version={version}");
        }

        if (include.HasValue)
        {
            queryParams.Add($"{includeType}={include.Value.ToString().ToLower()}");
        }

        if (queryParams.Count > 0)
        {
            url += $"?{string.Join("&", queryParams)}";
        }

        T response = await GetAsync<T>(url, cancellationToken);
        return response;
    }

    public async Task<SongList> GetSongListAsync(int? version = null, bool? includeNotes = null, CancellationToken cancellationToken = default) =>
        await GetListAsync<SongList>("song", "notes", version, includeNotes, cancellationToken);

    public async Task<Song> GetSongAsync(int id, int? version = null, CancellationToken cancellationToken = default)
    {
        string url = $"/api/v0/maimai/song/{id}";
        if (version.HasValue)
        {
            url += $"?version={version}";
        }

        return await GetAsync<Song>(url, cancellationToken);
    }

    public async Task<List<Alias>> GetAliasAsync(CancellationToken cancellationToken = default)
    {
        AliasListResponse response = await GetAsync<AliasListResponse>("/api/v0/maimai/alias/list", cancellationToken);
        return response.Aliases;
    }

    public async Task<List<Icon>> GetIconListAsync(int? version = null, bool? includeRequired = null, CancellationToken cancellationToken = default)
    {
        IconListResponse response = await GetListAsync<IconListResponse>("icon", "required", version, includeRequired, cancellationToken);
        return response.Icons;
    }

    public async Task<Icon> GetIconAsync(int id, int? version = null, CancellationToken cancellationToken = default)
    {
        string url = $"/api/v0/maimai/icon/{id}";
        if (version.HasValue)
        {
            url += $"?version={version}";
        }

        return await GetAsync<Icon>(url, cancellationToken);
    }

    public async Task<List<NamePlate>> GetPlateListAsync(int? version = null, bool? includeRequired = null, CancellationToken cancellationToken = default)
    {
        PlateListResponse response = await GetListAsync<PlateListResponse>("plate", "required", version, includeRequired, cancellationToken);
        return response.Plates;
    }

    public async Task<NamePlate> GetPlateAsync(int plateId, int? version = null, CancellationToken cancellationToken = default)
    {
        string url = $"/api/v0/maimai/plate/{plateId}";
        if (version.HasValue)
        {
            url += $"?version={version}";
        }

        return await GetAsync<NamePlate>(url, cancellationToken);
    }

    public async Task<List<Frame>> GetFrameListAsync(int? version = null, bool? includeRequired = null, CancellationToken cancellationToken = default)
    {
        FrameListResponse response = await GetListAsync<FrameListResponse>("frame", "required", version, includeRequired, cancellationToken);
        return response.Frames;
    }

    public async Task<Frame> GetFrameAsync(int frameId, int? version = null, CancellationToken cancellationToken = default)
    {
        string url = $"/api/v0/maimai/frame/{frameId}";
        if (version.HasValue)
        {
            url += $"?version={version}";
        }

        return await GetAsync<Frame>(url, cancellationToken);
    }

    public async Task<List<CollectionGenre>> GetCollectionGenreListAsync(int? version = null, CancellationToken cancellationToken = default)
    {
        string url = "/api/v0/maimai/collection-genre/list";
        if (version.HasValue)
        {
            url += $"?version={version}";
        }

        CollectionGenreListResponse response = await GetAsync<CollectionGenreListResponse>(url, cancellationToken);
        return response.CollectionGenres;
    }

    public async Task<CollectionGenre> GetCollectionGenreAsync(int id, int? version = null, CancellationToken cancellationToken = default)
    {
        string url = $"/api/v0/maimai/collection-genre/{id}";
        if (version.HasValue)
        {
            url += $"?version={version}";
        }

        return await GetAsync<CollectionGenre>(url, cancellationToken);
    }

    private record AliasListResponse
    {
        [JsonPropertyName("aliases")]
        public required List<Alias> Aliases { get; set; }
    }

    private record IconListResponse
    {
        [JsonPropertyName("icons")]
        public required List<Icon> Icons { get; set; }
    }

    private record PlateListResponse
    {
        [JsonPropertyName("plates")]
        public required List<NamePlate> Plates { get; set; }
    }

    private record FrameListResponse
    {
        [JsonPropertyName("frames")]
        public required List<Frame> Frames { get; set; }
    }

    private record CollectionGenreListResponse
    {
        [JsonPropertyName("collectionGenres")]
        public required List<CollectionGenre> CollectionGenres { get; set; }
    }
}
