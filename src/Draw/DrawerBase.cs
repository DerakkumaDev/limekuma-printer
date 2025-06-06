using Limekuma.Prober.Lxns;
using Limekuma.Prober.Lxns.Models;
using SixLabors.Fonts;

namespace Limekuma.Draw;

public abstract class DrawerBase
{
#if RELEASE
    public const string FontRootPath = "./Static/maimai/font/";
    public const string JacketRootPath = "./Cache/Jacket/";
    public const string RateRootPath = "./Static/Maimai/Rate/";
    public const string ComboRootPath = "./Static/Maimai/Bests/Combo/";
    public const string SyncRootPath = "./Static/Maimai/Bests/Sync/";
    public const string SongTypeRootPath = "./Static/maimai/MusicType/";
#elif DEBUG
    public const string FontRootPath = "./Resources/Font/";
    public const string JacketRootPath = "./Resources/Jacket/";
    public const string RateRootPath = "./Resources/Rate/";
    public const string ComboRootPath = "./Resources/Combo/";
    public const string SyncRootPath = "./Resources/Sync/";
    public const string SongTypeRootPath = "./Resources/SongType/";
#endif

    static DrawerBase()
    {
        FontCollection fonts = new();
        MediumFont = fonts.Add(Path.Combine(FontRootPath, "rounded-x-mplus-1p-medium.ttf"));
        BoldFont = fonts.Add(Path.Combine(FontRootPath, "rounded-x-mplus-1p-bold.ttf"));
        HeavyFont = fonts.Add(Path.Combine(FontRootPath, "rounded-x-mplus-1p-heavy.ttf"));

        NotoMediumFont = fonts.Add(Path.Combine(FontRootPath, "NotoSansCJKsc-Medium.otf"));
        NotoBoldFont = fonts.Add(Path.Combine(FontRootPath, "NotoSansCJKsc-Bold.otf"));
        NotoBlackFont = fonts.Add(Path.Combine(FontRootPath, "NotoSansCJKsc-Black.otf"));
        SymbolsFont = fonts.Add(Path.Combine(FontRootPath, "NotoSansSymbols-Regular.ttf"));
        Symbols2Font = fonts.Add(Path.Combine(FontRootPath, "NotoSansSymbols2-Regular.ttf"));
    }

    #region Font Families

    // Main fonts (Rounded-X M+ 1p from 自家製フォント工房)
    protected static FontFamily MediumFont { get; }
    protected static FontFamily BoldFont { get; }
    protected static FontFamily HeavyFont { get; }

    // Fallback fonts (思源黑体/Noto Sans from Adobe & Google)
    protected static FontFamily NotoMediumFont { get; }
    protected static FontFamily NotoBoldFont { get; }
    protected static FontFamily NotoBlackFont { get; }
    protected static FontFamily SymbolsFont { get; }
    protected static FontFamily Symbols2Font { get; }

    #endregion

    protected static SongList SongList
    {
        get
        {
            if (field is null || DateTimeOffset.Now.AddHours(10).Date != field.PullTime.AddHours(10).Date)
            {
                LxnsResourceClient _resource = new();
                field = _resource.GetSongListAsync(includeNotes: true).Result;
            }

            return field;
        }
    }
}