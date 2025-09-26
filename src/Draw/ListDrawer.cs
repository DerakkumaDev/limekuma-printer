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

    public Image Draw(CommonUser user, IList<CommonRecord> records, int page, int total, IList<int> counts,
        string level, string backgroundPath = BackgroundPath)
    {
        Image bg = Image.Load(backgroundPath);
        using Image recordsImage = DrawScores(records);
        using Image frameImage = Image.Load(FramePath);
        using Image levelImage = Image.Load(Path.Combine(LevelRootPath, $"{level}.png"));
        using Image plate = Image.Load(Path.Combine(PlateRootPath, $"{user.PlateId}.png"));
        using Image iconImage = Image.Load(Path.Combine(IconRootPath, $"{user.IconId}.png"));
        using Image @class = Image.Load(Path.Combine(ClassRootPath, $"{(int)user.ClassRank}.png"));
        using Image course = Image.Load(Path.Combine(DaniRootPath, $"{(int)user.CourseRank}.png"));
        using Image shougoubase = Image.Load(Path.Combine(ShougouRootPath, $"{user.TrophyColor}.png"));
        using Image frameLine = Image.Load(FrameLinePath);
        using Image namebase = Image.Load(NamebasePath);
        using Image ratingbase = Image.Load(Path.Combine(RatingRootPath, $"{user.RatingLevel}.png"));

        frameImage.Resize(0.95, KnownResamplers.Lanczos3);
        levelImage.Resize(0.7, KnownResamplers.Lanczos3);
        plate.Resize(0.951, KnownResamplers.Lanczos3);
        iconImage.Resize(0.737, KnownResamplers.Lanczos3);
        ratingbase.Resize(0.948, KnownResamplers.Lanczos3);
        @class.Resize(0.775, KnownResamplers.Lanczos3);
        course.Resize(0.215, KnownResamplers.Lanczos3);
        shougoubase.Resize(0.94, KnownResamplers.Lanczos3);
        frameLine.Resize(0.745, KnownResamplers.Lanczos3);

        Font robinEbFont16 = RobinEbFont.GetSizeFont(33);

        List<char> ratingLE = [.. user.Rating.ToString().Reverse()];
        using Image<Rgba32> ratingImage = new(512, 64);
        ratingImage.Mutate(ctx =>
        {
            ReadOnlySpan<int> ratingPos = [111, 82, 55, 26, 0];
            for (int i = 0; i < ratingLE.Count; ++i)
            {
                ctx.DrawText(ratingLE[i].ToString(), robinEbFont16, Brushes.Solid(new Rgb24(249, 198, 10)),
                    Pens.Solid(new Rgba32(0, 0, 0, 150), 1f), new(ratingPos[i], 20));
            }

            ctx.Resize(ratingImage.Width / 2, ratingImage.Height / 2, KnownResamplers.Spline);
        });

        string shougou = user.TrophyText;
        FontRectangle shougouSize = HeavyFont.GetSize(14, shougou, [SymbolsFont, Symbols2Font, NotoBlackFont]);
        Point shougoubasePos = new(181, 143);
        PointF shougouPos = new(shougoubasePos.X + ((shougoubase.Width - shougouSize.Width) / 2), 151);
        using Image shougouImage = HeavyFont.DrawImage(14, shougou, Brushes.Solid(new Rgb24(255, 255, 255)),
            Pens.Solid(new Rgb24(0, 0, 0), 5f), [SymbolsFont, Symbols2Font, NotoBlackFont], KnownResamplers.Spline, 6);

        string pagination = $"{page} / {total}";
        FontRectangle paginationSize = HeavyFont.GetSize(70, pagination, [SymbolsFont, Symbols2Font, NotoBlackFont]);
        PointF paginationPos = new(256 - (paginationSize.Width / 2), 815);
        using Image paginationImage = HeavyFont.DrawImage(70, pagination, new Color(new Rgb24(53, 74, 164)),
            [SymbolsFont, Symbols2Font, NotoBlackFont], KnownResamplers.Lanczos3);

        using Image nameImage = MediumFont.DrawImage(21, user.Name, new Color(new Rgb24(0, 0, 0)),
            [SymbolsFont, Symbols2Font, NotoMediumFont], KnownResamplers.Lanczos3);

        bg.Mutate(ctx =>
        {
            ctx.DrawImage(frameImage, new Point(48, 45), 1);
            ctx.DrawImage(levelImage, new Point(755 - (level.Length * 8), 45), 1);
            ctx.DrawImage(plate, new Point(76, 69), 1);
            ctx.DrawImage(namebase, new Point(183, 108), 1);
            ctx.DrawImage(ratingbase, new Point(180, 72), 1);
            ctx.DrawImage(shougoubase, shougoubasePos, 1);
            ctx.DrawImage(iconImage, new Point(85, 76), 1);
            ctx.DrawImage(ratingImage, new Point(264, 81), 1);
            ctx.DrawImage(nameImage, new Point(190, 118), 1);
            ctx.DrawImage(course, new Point(357, 109), 1);
            ctx.DrawImage(@class, new Point(342, 49), 1);
            ctx.DrawImage(shougouImage, (Point)shougouPos, 1);
            ctx.DrawImage(paginationImage, (Point)paginationPos, 1);
            ctx.DrawImage(frameLine, new Point(40, 36), 1);
            ctx.DrawImage(recordsImage, new Point(25, 800), 1);
        });

        int index = 0;
        int countsCount = counts.Count - 1;
        int totalCount = counts[^1];
        Point point = new(200, 264);
        for (int columnIndex = 0; index < countsCount; ++columnIndex)
        {
            for (int rowIndex = 0, rowMaxIndex = columnIndex == 0 ? 7 : 8;
                 rowIndex < rowMaxIndex && index < countsCount;
                 ++index, ++rowIndex)
            {
                int count = counts[index];
                string countText = $"{count}/{totalCount}";
                FontRectangle countSize = BoldFont.GetSize(20, countText, [SymbolsFont, Symbols2Font, NotoBoldFont]);
                PointF countPos = new(point.X - (countSize.Width / 2), point.Y);
                using Image countImage = BoldFont.DrawImage(20, countText,
                    new Color(count >= totalCount ? new(248, 179, 42) : new Rgb24(53, 74, 164)),
                    [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
                bg.Mutate(ctx => ctx.DrawImage(countImage, (Point)countPos, 1));

                point.X += columnIndex == 0 ? 120 : 102;
            }

            point.X = 200;
            point.Y += 90;
        }

        return bg;
    }
}