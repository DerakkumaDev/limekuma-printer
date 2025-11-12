using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;

namespace Limekuma.Utils;

public sealed class AssetManager
{
    private readonly ConcurrentDictionary<string, Image> _assets;

    static AssetManager() => Shared = new();

    private AssetManager() => _assets = [];
    public static AssetManager Shared { get; private set; }

    public Image Load(string path)
    {
        if (!_assets.TryGetValue(path, out Image? image))
        {
            image = Image.Load(path);
            _assets.TryAdd(path, image);
        }

        return image.Clone(x => { });
    }
}