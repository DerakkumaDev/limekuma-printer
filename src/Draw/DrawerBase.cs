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
    public const string ProberRootPath = "./Static/Maimai/Prober/";
    public const string ProberLogoRootPath = "./Static/Maimai/ProberLogo/";
#elif DEBUG
    public const string FontRootPath = "./Resources/Font/";
    public const string JacketRootPath = "./Resources/Jacket/";
    public const string RateRootPath = "./Resources/Rate/";
    public const string ComboRootPath = "./Resources/Combo/";
    public const string SyncRootPath = "./Resources/Sync/";
    public const string SongTypeRootPath = "./Resources/SongType/";
    public const string ProberRootPath = "./Resources/Prober/";
    public const string ProberLogoRootPath = "./Resources/ProberLogo/";
#endif

    static DrawerBase()
    {
        LatinMediumFont = new FontCollection().Add(Path.Combine(FontRootPath, "SF-Pro-Rounded-Medium.otf"));
        LatinBoldFont = new FontCollection().Add(Path.Combine(FontRootPath, "SF-Pro-Rounded-Bold.otf"));
        LatinHeavyFont = new FontCollection().Add(Path.Combine(FontRootPath, "SF-Pro-Rounded-Heavy.otf"));

        JpMediumFont = new FontCollection().Add(Path.Combine(FontRootPath, "rounded-mgenplus-1c-medium.ttf"));
        JpBoldFont = new FontCollection().Add(Path.Combine(FontRootPath, "rounded-mgenplus-1c-bold.ttf"));
        JpHeavyFont = new FontCollection().Add(Path.Combine(FontRootPath, "rounded-mgenplus-1c-heavy.ttf"));

        ScMediumFont = new FontCollection().Add(Path.Combine(FontRootPath, "STYuantiRegular.ttf"));
        ScBoldFont = new FontCollection().Add(Path.Combine(FontRootPath, "STYuantiBold.ttf"));
        //ScHeavyFont = new FontCollection().Add(Path.Combine(FontRootPath, "STYuantiBold.ttf"));
        ScHeavyFont = ScBoldFont;

        SymbolsFont = new FontCollection().Add(Path.Combine(FontRootPath, "NotoSansSymbols-Regular.ttf"));
        Symbols2Font = new FontCollection().Add(Path.Combine(FontRootPath, "NotoSansSymbols2-Regular.ttf"));

        RobinEbFont = new FontCollection().Add(Path.Combine(FontRootPath, "3b02d0e2b846ab130c78c93bec66bf26.otf"));
    }

    #region Font Families

    // Latin Fonts (SF Rounded from Apple)
    protected static FontFamily LatinMediumFont { get; }
    protected static FontFamily LatinBoldFont { get; }
    protected static FontFamily LatinHeavyFont { get; }

    // Jepanese Fonts (ヒラギノ丸ゴ from SCREEN GA)
    protected static FontFamily JpMediumFont { get; }
    protected static FontFamily JpBoldFont { get; }
    protected static FontFamily JpHeavyFont { get; }

    // Chinese Fonts (华文圆体 from 常州华文)
    protected static FontFamily ScMediumFont { get; }
    protected static FontFamily ScBoldFont { get; }
    protected static FontFamily ScHeavyFont { get; }

    // Symbol Fonts (Noto Sans from Adobe & Google)
    protected static FontFamily SymbolsFont { get; }
    protected static FontFamily Symbols2Font { get; }

    // Number fonts (ロダン from Fontworks)
    protected static FontFamily RobinEbFont { get; }

    #endregion
}