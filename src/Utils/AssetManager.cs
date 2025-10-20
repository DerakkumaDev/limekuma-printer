using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;

namespace Limekuma.Utils;

public class AssetManager
{
    public static AssetManager Shared { get; private set; }

    static AssetManager()
    {
        Shared = new(); 
    }

    AssetManager()
    {
        _assets = [];
    }

    private readonly ConcurrentDictionary<string, Image> _assets;

    public Image Load(string path)
    {
        if (!_assets.TryGetValue(path, out Image? image))
        {
            image = Image.Load(path);
            _assets.GetOrAdd(path, image);
        }

        return image.Clone(x => { });
    }
}
