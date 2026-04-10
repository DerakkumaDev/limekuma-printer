using Limekuma.Prober.Common;
using Limekuma.Render;
using System.Reflection;

namespace Limekuma.Utils;

internal static class ServiceHelper
{
    private static readonly HttpClient Http = BuildHttpClient();

    private static readonly ParallelOptions PrepareRecordParallelOptions = new()
    {
        MaxDegreeOfParallelism = 16
    };

    internal static async Task PrepareUserDataAsync(CommonUser user) => await Task.WhenAll(
        PrepareImageAsset("Icon", user.IconId, user.IconUrl),
        PrepareImageAsset("Plate", user.PlateId, user.PlateUrl),
        PrepareImageAsset("Frame", user.FrameId, user.FrameUrl));

    internal static async Task PrepareRecordDataAsync(IReadOnlyList<CommonRecord> records) =>
        await Parallel.ForEachAsync(records, PrepareRecordParallelOptions,
            async (record, _) => await PrepareRecordDataAsync(record));

    internal static async Task PrepareRecordDataAsync(CommonRecord record) =>
        await PrepareImageAsset("Jacket", record.Chart.Song.Id % 10000, record.Chart.Song.JacketUrl);

    private static async Task PrepareImageAsset(string assetKey, int id, string url)
    {
        string? basePath = AssetProvider.Shared.GetPath(assetKey);
        if (string.IsNullOrEmpty(basePath))
        {
            return;
        }

        string path = Path.Combine(basePath, $"{id}.png");
        if (File.Exists(path))
        {
            return;
        }

        Directory.CreateDirectory(basePath);

        await using Stream stream = await Http.GetStreamAsync(url);
        await using FileStream fileStream = File.OpenWrite(path);
        await stream.CopyToAsync(fileStream);
    }

    private static HttpClient BuildHttpClient()
    {
        HttpClient http = new();
        http.DefaultRequestHeaders.UserAgent.Add(new("limekuma",
            Assembly.GetExecutingAssembly().GetName().Version?.ToString()));
        return http;
    }
}
