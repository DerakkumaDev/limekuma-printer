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

    public Image Draw(CommonUser user, IReadOnlyList<CommonRecord> records, int page, int total, IReadOnlyList<int> counts,
        string level, string prober, string backgroundPath = BackgroundPath)
    {
        Image bg = AssetManager.Shared.Load(backgroundPath);
        List<(Point, Image)> recordImages = DrawScores(records);
        using Image frameImage = AssetManager.Shared.Load(FramePath);
        using Image levelImage = AssetManager.Shared.Load(Path.Combine(LevelRootPath, $"{level}.png"));
        using Image plate = AssetManager.Shared.Load(Path.Combine(PlateRootPath, $"{user.PlateId}.png"));
        using Image iconImage = AssetManager.Shared.Load(Path.Combine(IconRootPath, $"{user.IconId}.png"));
        using Image @class = AssetManager.Shared.Load(Path.Combine(ClassRootPath, $"{(int)user.ClassRank}.png"));
        using Image course = AssetManager.Shared.Load(Path.Combine(DaniRootPath, $"{(int)user.CourseRank}.png"));
        using Image shougoubase = AssetManager.Shared.Load(Path.Combine(ShougouRootPath, $"{user.TrophyColor}.png"));
        using Image frameLine = AssetManager.Shared.Load(FrameLinePath);
        using Image namebase = AssetManager.Shared.Load(NamebasePath);
        using Image ratingbase = AssetManager.Shared.Load(Path.Combine(RatingRootPath, $"{user.RatingLevel}.png"));
        using Image proberLogo = AssetManager.Shared.Load(Path.Combine(ProberLogoRootPath, $"{prober}.png"));

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

        using Image<Rgba32> ratingImage = new(512, 64);
        ratingImage.Mutate(ctx =>
        {
            ReadOnlySpan<int> ratingPos = [111, 82, 55, 26, 0];
            ReadOnlySpan<char> ratingLE = [.. user.Rating.ToString().Reverse()];
            for (int i = 0; i < ratingLE.Length; ++i)
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
        using Image paginationImage = HeavyFont.DrawImage(70, pagination, new(new Rgb24(53, 74, 164)),
            [SymbolsFont, Symbols2Font, NotoBlackFont], KnownResamplers.Lanczos3);

        using Image nameImage = MediumFont.DrawImage(21, user.Name, new(new Rgb24(0, 0, 0)),
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
            ctx.DrawImage(nameImage, new Point(190, 116), 1);
            ctx.DrawImage(course, new Point(357, 109), 1);
            ctx.DrawImage(@class, new Point(342, 49), 1);
            ctx.DrawImage(shougouImage, (Point)shougouPos, 1);
            ctx.DrawImage(paginationImage, (Point)paginationPos, 1);
            ctx.DrawImage(frameLine, new Point(40, 36), 1);
            ctx.DrawImage(proberLogo, new Point(1011, 407), 1);
            foreach ((Point point, Image recordImage) in recordImages)
            {
                using (recordImage)
                {
                    Point realPoint = point;
                    realPoint.X += 25;
                    realPoint.Y += 800;
                    ctx.DrawImage(recordImage, realPoint, 1);
                }
            }
        });

        int countsCount = counts.Count - 1;
        int totalCount = counts[^1];

        (PointF position, Image image)[] statsData = new (PointF, Image)[countsCount];

        Parallel.For(0, countsCount, idx =>
        {
            int count = counts[idx];
            string countText = $"{count}/{totalCount}";
            FontRectangle countSize = BoldFont.GetSize(20, countText, [SymbolsFont, Symbols2Font, NotoBoldFont]);

            PointF countPos;
            if (idx < 7)
            {
                countPos = new(200 - (countSize.Width / 2) + (idx * 120), 264);
            }
            else
            {
                int remain = idx - 7;
                int col = remain / 8;
                int row = remain % 8;
                countPos = new(200 - (countSize.Width / 2) + (row * 102), 264 + ((col + 1) * 90));
            }

            Image countImage = BoldFont.DrawImage(20, countText,
                new(count >= totalCount ? new(248, 179, 42) : new Rgb24(53, 74, 164)),
                [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);

            statsData[idx] = (countPos, countImage);
        });

        foreach ((PointF position, Image image) in statsData)
        {
            using (image)
            {
                bg.Mutate(ctx => ctx.DrawImage(image, (Point)position, 1));
            }
        }

        return bg;
    }
}