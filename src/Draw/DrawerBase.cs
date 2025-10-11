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
    public const string ProberLogoRootPath = "./Static/Maimai/ProberLogo/";
#elif DEBUG
    public const string FontRootPath = "./Resources/Font/";
    public const string JacketRootPath = "./Resources/Jacket/";
    public const string RateRootPath = "./Resources/Rate/";
    public const string ComboRootPath = "./Resources/Combo/";
    public const string SyncRootPath = "./Resources/Sync/";
    public const string SongTypeRootPath = "./Resources/SongType/";
    public const string ProberLogoRootPath = "./Resources/ProberLogo/";
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

        RobinEbFont = fonts.Add(Path.Combine(FontRootPath, "3b02d0e2b846ab130c78c93bec66bf26.otf"));
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

    // Number fonts (FOT-Rodin ProN from Fontworks)
    protected static FontFamily RobinEbFont { get; }

    #endregion
}