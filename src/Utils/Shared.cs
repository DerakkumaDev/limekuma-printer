namespace DXKumaBot.Backend.Utils;

public static class Shared
{
#if RELEASE
    public const string FontRootPath = "./Static/maimai/font/";
    public const string IconRootPath = "./Static/maimai/Icon/";
    public const string PlateRootPath = "./Cache/Plate/";
    public const string FrameRootPath = "./Static/maimai/Frame/";
    public const string JacketRootPath = "./Cache/Jacket/";
    public const string PartRootPath = "./Static/Maimai/Best50/Part/";
    public const string SongTypeRootPath = "./Static/maimai/MusicType/";
    public const string RateRootPath = "./Static/Maimai/Rate/";
    public const string DxStarRootPath = "./Static/maimai/DXScoreStar/";
    public const string ComboRootPath = "./Static/Maimai/Best50/Combo/";
    public const string SyncRootPath = "./Static/Maimai/Best50/Sync/";
    public const string ClassRootPath = "./Static/maimai/Class/";
    public const string DaniRootPath = "./Static/maimai/Dani/";
    public const string ShougouRootPath = "./Static/maimai/Shougou/";
    public const string RatingRootPath = "./Static/Maimai/Rating/";
    public const string FrameLinePath = "./Static/Maimai/Best50/frame.png";
    public const string NamebasePath = "./Static/Maimai/Best50/namebase.png";
    public const string ScorebasePath = "./Static/Maimai/Best50/scorebase.png";
    public const string BackgroundPath = "./Static/Maimai/Best50/background.png";
    public const string BackgroundAnimationPath = "./Static/Maimai/Best50/background_animation.gif";
#elif DEBUG
    public const string FontRootPath = "./Resources/Font/";
    public const string IconRootPath = "./Resources/Icon/";
    public const string PlateRootPath = "./Resources/Plate/";
    public const string FrameRootPath = "./Resources/Frame/";
    public const string JacketRootPath = "./Resources/Jacket/";
    public const string PartRootPath = "./Resources/Part/";
    public const string SongTypeRootPath = "./Resources/SongType/";
    public const string RateRootPath = "./Resources/Rate/";
    public const string DxStarRootPath = "./Resources/DXStar/";
    public const string ComboRootPath = "./Resources/Combo/";
    public const string SyncRootPath = "./Resources/Sync/";
    public const string ClassRootPath = "./Resources/Class/";
    public const string DaniRootPath = "./Resources/Dani/";
    public const string ShougouRootPath = "./Resources/Shougou/";
    public const string RatingRootPath = "./Resources/Rating/";
    public const string FrameLinePath = "./Resources/frame.png";
    public const string NamebasePath = "./Resources/namebase.png";
    public const string ScorebasePath = "./Resources/scorebase.png";
    public const string BackgroundPath = "./Resources/background.png";
    public const string BackgroundAnimationPath = "./Resources/background_animation.gif";
#endif
}
