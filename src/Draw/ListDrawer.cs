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

    public Image Draw(CommonUser user, IReadOnlyList<CommonRecord> records, int page, int total,
        IReadOnlyList<int> counts, string level, string prober, string backgroundPath = BackgroundPath)
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
        string proberState;
        string? proberStateDesc = null;
        if (records.Any(r => r.DXScore is 0 && (r.DXStar > 0 || r.Rank < Ranks.C)))
        {
            proberState = "warning";
            proberStateDesc = "查分器可能启用了掩码";
        }
        else
        {
            proberState = "on";
        }

        using Image proberbase = AssetManager.Shared.Load(Path.Combine(ProberRootPath, $"{proberState}.png"));

        frameImage.Resize(0.95, KnownResamplers.Lanczos3);
        levelImage.Resize(0.7, KnownResamplers.Lanczos3);
        plate.Resize(0.951, KnownResamplers.Lanczos3);
        iconImage.Resize(0.737, KnownResamplers.Lanczos3);
        ratingbase.Resize(0.948, KnownResamplers.Lanczos3);
        @class.Resize(0.775, KnownResamplers.Lanczos3);
        course.Resize(0.215, KnownResamplers.Lanczos3);
        shougoubase.Resize(0.94, KnownResamplers.Lanczos3);
        frameLine.Resize(0.745, KnownResamplers.Lanczos3);

        Font robinEbFont16 = RobinEbFont.GetSizeFont(4);

        using Image<Rgba32> ratingImage = new(256, 32);
        ratingImage.Mutate(ctx =>
        {
            ReadOnlySpan<int> ratingPos = [56, 41, 27, 13, 0];
            ReadOnlySpan<char> ratingLE = [.. user.Rating.ToString().Reverse()];
            for (int i = 0; i < ratingLE.Length; ++i)
            {
                RichTextOptions options = new(robinEbFont16)
                {
                    Dpi = 300,
                    Origin = new(ratingPos[i], 10)
                };
                ctx.DrawText(options, ratingLE[i].ToString(), Brushes.Solid(new Rgb24(249, 198, 10)),
                    Pens.Solid(new Rgba32(0, 0, 0, 100), 1.33f));
            }
        });

        string shougou = user.TrophyText;
        FontRectangle shougouSize =
            LatinHeavyFont.GetSize(14, shougou, [JpHeavyFont, ScHeavyFont, SymbolsFont, Symbols2Font]);
        Point shougoubasePos = new(180, 143);
        PointF shougouPos = new(shougoubasePos.X + ((shougoubase.Width - shougouSize.Width) / 2), 151);
        using Image shougouImage = LatinHeavyFont.DrawImage(14, shougou, Brushes.Solid(new Rgb24(255, 255, 255)),
            Pens.Solid(new Rgba32(0, 0, 0, 200), 1.5f), [JpHeavyFont, ScHeavyFont, SymbolsFont, Symbols2Font]);

        string pagination = $"{page} / {total}";
        FontRectangle paginationSize =
            LatinHeavyFont.GetSize(70, pagination, [JpHeavyFont, ScHeavyFont, SymbolsFont, Symbols2Font]);
        PointF paginationPos = new(256 - (paginationSize.Width / 2), 815);
        using Image paginationImage = LatinHeavyFont.DrawImage(70, pagination, new(new Rgb24(53, 74, 164)),
            [JpHeavyFont, ScHeavyFont, SymbolsFont, Symbols2Font]);

        using Image nameImage = LatinMediumFont.DrawImage(21, user.Name, new(new Rgb24(0, 0, 0)),
            [JpMediumFont, ScMediumFont, SymbolsFont, Symbols2Font]);

        if (proberStateDesc is not null)
        {
            using Image proberStateDescbase = AssetManager.Shared.Load(ProberStateDescbasePath);
            FontRectangle proberStateDescSize = LatinBoldFont.GetSize(27, proberStateDesc,
                [JpBoldFont, ScBoldFont, SymbolsFont, Symbols2Font]);
            PointF proberStateDescPos = new(834 - (proberStateDescSize.Width / 2), 528);
            Rgb24 proberStateDescColorValue = new(75, 77, 138);
            Color proberStateDescColor = new(proberStateDescColorValue);
            using Image proberStateDescImage = LatinBoldFont.DrawImage(27, proberStateDesc, proberStateDescColor,
                [JpBoldFont, ScBoldFont, SymbolsFont, Symbols2Font]);
            bg.Mutate(ctx =>
            {
                ctx.DrawImage(proberStateDescbase, new Point(574, 492), 1);
                ctx.DrawImage(proberStateDescImage, (Point)proberStateDescPos, 1);
            });
        }

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
            ctx.DrawImage(proberbase, new Point(1011, 407), 1);
            ctx.DrawImage(proberLogo, new Point(1015, 411), 1);
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
            FontRectangle countSize =
                LatinBoldFont.GetSize(20, countText, [JpBoldFont, ScBoldFont, SymbolsFont, Symbols2Font]);

            PointF countPos;
            if (idx < 7)
            {
                countPos = new(200 - (countSize.Width / 2) + (idx * 120), 250);
            }
            else
            {
                int remain = idx - 7;
                int col = remain / 8;
                int row = remain % 8;
                countPos = new(200 - (countSize.Width / 2) + (row * 102), 250 + ((col + 1) * 90));
            }

            Image countImage = LatinBoldFont.DrawImage(20, countText,
                new(count >= totalCount ? new(248, 179, 42) : new Rgb24(53, 74, 164)),
                [JpBoldFont, ScBoldFont, SymbolsFont, Symbols2Font]);

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