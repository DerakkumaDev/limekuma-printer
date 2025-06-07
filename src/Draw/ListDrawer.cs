using Limekuma.Prober.Common;
using Limekuma.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Limekuma.Draw;

public class ListDrawer : BestsDrawer
{
#if RELEASE
    public new const string BackgroundPath = "./Static/Maimai/List/background.png";
    public const string FramePath = "./Static/Maimai/List/frame.png";
    public const string LevelRootPath = "./Static/maimai/Level";
#elif DEBUG
    public new const string BackgroundPath = "./Resources/Background/list.png";
    public const string FramePath = "./Resources/Background/list_frame.png";
    public const string LevelRootPath = "./Resources/Level/";
#endif

    public Image Draw(CommonUser user, List<CommonRecord> records, int page, int total, IEnumerable<int> counts,
        string level, string backgroundPath = BackgroundPath)
    {
        using Image recordsImage = DrawScores(records);
        using Image<Rgba32> image = new(1440, 2560);
        using Image frameImage = Image.Load(FramePath);
        using Image levelImage = Image.Load(Path.Combine(LevelRootPath, $"{level}.png"));
        using Image plate = Image.Load(Path.Combine(PlateRootPath, $"{user.PlateId.ToString().PadLeft(6, '0')}.png"));
        using Image iconImage = Image.Load(Path.Combine(IconRootPath, $"{user.IconId}.png"));
        using Image @class = Image.Load(Path.Combine(ClassRootPath, $"{(int)user.ClassRank}.png"));
        using Image course = Image.Load(Path.Combine(DaniRootPath, $"{(int)user.CourseRank}.png"));
        using Image shougoubase = Image.Load(Path.Combine(ShougouRootPath, $"{user.TrophyColor}.png"));
        using Image frameLine = Image.Load(FrameLinePath);
        using Image namebase = Image.Load(NamebasePath);
        using Image ratingbase = Image.Load(Path.Combine(RatingRootPath, $"{user.RatingLevel}.png"));

        frameImage.Resize(0.95, KnownResamplers.Lanczos3);
        levelImage.Resize(0.7, KnownResamplers.Lanczos3);
        plate.Resize(0.95, KnownResamplers.Lanczos3);
        iconImage.Resize(0.75, KnownResamplers.Lanczos3);
        ratingbase.Resize(0.96, KnownResamplers.Lanczos3);
        @class.Resize(0.245, KnownResamplers.Lanczos3);
        course.Resize(0.213, KnownResamplers.Lanczos3);
        shougoubase.Resize(0.94, KnownResamplers.Lanczos3);
        frameLine.Resize(0.745, KnownResamplers.Lanczos3);

        Font notoBlackFont16 = NotoBlackFont.GetSizeFont(32);

        List<char> ratingLE = [.. user.Rating.ToString().Reverse()];
        using Image<Rgba32> ratingImage = new(512, 64);
        ratingImage.Mutate(ctx =>
        {
            ReadOnlySpan<int> ratingPos = [114, 86, 56, 28, 0];
            for (int i = 0; i < ratingLE.Count; ++i)
            {
                ctx.DrawText(ratingLE[i].ToString(), notoBlackFont16, new Color(new Rgb24(249, 198, 10)),
                    new(ratingPos[i], 0));
            }

            ctx.Resize(ratingImage.Width * 16 / 32, ratingImage.Height * 16 / 32, KnownResamplers.Spline);
        });

        string shougou = user.TrophyText;
        FontRectangle shougouSize = HeavyFont.GetSize(14, shougou, [SymbolsFont, Symbols2Font, NotoBlackFont]);
        Point shougoubasePos = new(181, 143);
        PointF shougouPos = new(shougoubasePos.X + ((shougoubase.Width - shougouSize.Width) / 2), 151);

        string pagination = $"{page} / {total}";
        FontRectangle paginationSize = HeavyFont.GetSize(70, pagination, [SymbolsFont, Symbols2Font, NotoBlackFont]);
        PointF paginationPos = new(256 - (paginationSize.Width / 2), 815);

        using Image shougouImage = HeavyFont.DrawImage(14, shougou, Brushes.Solid(new Rgb24(255, 255, 255)),
            Pens.Solid(new Rgb24(0, 0, 0), 2f), [SymbolsFont, Symbols2Font, NotoBlackFont], KnownResamplers.Spline);
        using Image nameImage = MediumFont.DrawImage(22, user.Name, new Color(new Rgb24(0, 0, 0)),
            [SymbolsFont, Symbols2Font, NotoMediumFont], KnownResamplers.Lanczos3);
        using Image paginationImage = HeavyFont.DrawImage(70, pagination, new Color(new Rgb24(53, 74, 164)),
            [SymbolsFont, Symbols2Font, NotoBlackFont], KnownResamplers.Lanczos3);

        image.Mutate(ctx =>
        {
            ctx.DrawImage(frameImage, new Point(48, 45), 1);
            ctx.DrawImage(levelImage, new Point(755 - (level.Length * 8), 45), 1);
            ctx.DrawImage(plate, new Point(77, 69), 1);
            ctx.DrawImage(namebase, new Point(183, 108), 1);
            ctx.DrawImage(nameImage, new Point(190, 116), 1);
            ctx.DrawImage(iconImage, new Point(85, 76), 1);
            ctx.DrawImage(ratingbase, new Point(181, 73), 1);
            ctx.DrawImage(ratingImage, new Point(266, 82), 1);
            ctx.DrawImage(@class, new Point(344, 50), 1);
            ctx.DrawImage(course, new Point(358, 109), 1);
            ctx.DrawImage(shougoubase, shougoubasePos, 1);
            ctx.DrawImage(shougouImage, (Point)shougouPos, 1);
            ctx.DrawImage(paginationImage, (Point)paginationPos, 1);
            ctx.DrawImage(frameLine, new Point(40, 36), 1);
            ctx.DrawImage(recordsImage, new Point(25, 795), 1);
        });

        Image bg = Image.Load(backgroundPath);
        bg.Mutate(ctx => ctx.DrawImage(image, new Point(0, 0), 1));

        return bg;
    }
}