using Limekuma.Prober.Common;
using Limekuma.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Limekuma.Draw;

public class BestsDrawer : DrawerBase
{
#if RELEASE
    public const string IconRootPath = "./Static/maimai/Icon/";
    public const string PlateRootPath = "./Cache/Plate/";
    public const string FrameRootPath = "./Static/maimai/Frame/";
    public const string PartRootPath = "./Static/Maimai/Bests/Part/";
    public const string DxStarRootPath = "./Static/maimai/DXScoreStar/";
    public const string ClassRootPath = "./Static/maimai/Class/";
    public const string DaniRootPath = "./Static/maimai/Dani/";
    public const string ShougouRootPath = "./Static/maimai/Shougou/";
    public const string RatingRootPath = "./Static/Maimai/Rating/";
    public const string FrameLinePath = "./Static/Maimai/Bests/frame.png";
    public const string NamebasePath = "./Static/Maimai/Bests/namebase.png";
    public const string ScorebasePath = "./Static/Maimai/Bests/scorebase.png";
    public const string BackgroundPath = "./Static/Maimai/Bests/background.png";
    public const string BackgroundAnimationPath = "./Static/Maimai/Bests/background_animation.gif";
#elif DEBUG
    public const string IconRootPath = "./Resources/Icon/";
    public const string PlateRootPath = "./Resources/Plate/";
    public const string FrameRootPath = "./Resources/Frame/";
    public const string PartRootPath = "./Resources/Part/";
    public const string DxStarRootPath = "./Resources/DXStar/";
    public const string ClassRootPath = "./Resources/Class/";
    public const string DaniRootPath = "./Resources/Dani/";
    public const string ShougouRootPath = "./Resources/Shougou/";
    public const string RatingRootPath = "./Resources/Rating/";
    public const string FrameLinePath = "./Resources/Background/frame.png";
    public const string NamebasePath = "./Resources/Background/namebase.png";
    public const string ScorebasePath = "./Resources/Background/scorebase.png";
    public const string BackgroundPath = "./Resources/Background/bests.png";
    public const string BackgroundAnimationPath = "./Resources/Background/bests.gif";
#endif

    public Image Draw(CommonUser user, IList<CommonRecord> ever, IList<CommonRecord> current, int everTotal,
        int currentTotal, string typename, string backgroundPath = BackgroundPath)
    {
        using Image sdBests = DrawScores(ever);
        using Image dxBests = DrawScores(current, ever.Count);
        using Image<Rgba32> image = new(1440, 2560);
        using Image frameImage =
            Image.Load(Path.Combine(FrameRootPath, $"UI_Frame_{user.FrameId.ToString().PadLeft(6, '0')}.png"));
        using Image plate = Image.Load(Path.Combine(PlateRootPath, $"{user.PlateId}.png"));
        using Image iconImage = Image.Load(Path.Combine(IconRootPath, $"{user.IconId}.png"));
        using Image @class = Image.Load(Path.Combine(ClassRootPath, $"{(int)user.ClassRank}.png"));
        using Image course = Image.Load(Path.Combine(DaniRootPath, $"{(int)user.CourseRank}.png"));
        using Image shougoubase = Image.Load(Path.Combine(ShougouRootPath, $"{user.TrophyColor}.png"));
        using Image frameLine = Image.Load(FrameLinePath);
        using Image namebase = Image.Load(NamebasePath);
        using Image scorebase = Image.Load(ScorebasePath);
        using Image ratingbase = Image.Load(Path.Combine(RatingRootPath, $"{user.RatingLevel}.png"));

        frameImage.Resize(0.95, KnownResamplers.Lanczos3);
        plate.Resize(0.95, KnownResamplers.Lanczos3);
        iconImage.Resize(0.73, KnownResamplers.Lanczos3);
        ratingbase.Resize(0.945, KnownResamplers.Lanczos3);
        @class.Resize(0.78, KnownResamplers.Lanczos3);
        course.Resize(0.213, KnownResamplers.Lanczos3);
        shougoubase.Resize(0.94, KnownResamplers.Lanczos3);
        frameLine.Resize(0.745, KnownResamplers.Lanczos3);

        Font robinEbFont16 = RobinEbFont.GetSizeFont(32);

        List<char> ratingLE = [.. user.Rating.ToString().Reverse()];
        using Image<Rgba32> ratingImage = new(512, 64);
        ratingImage.Mutate(ctx =>
        {
            ReadOnlySpan<int> ratingPos = [114, 86, 56, 28, 0];
            for (int i = 0; i < ratingLE.Count; i++)
            {
                ctx.DrawText(ratingLE[i].ToString(), robinEbFont16, new Color(new Rgb24(249, 198, 10)),
                    new(ratingPos[i], 20));
            }

            ctx.Resize(ratingImage.Width * 16 / 32, ratingImage.Height * 16 / 32, KnownResamplers.Spline);
        });

        string shougou = user.TrophyText;
        FontRectangle shougouSize = HeavyFont.GetSize(14, shougou, [SymbolsFont, Symbols2Font, NotoBlackFont]);
        Point shougoubasePos = new(181, 143);
        PointF shougouPos = new(shougoubasePos.X + ((shougoubase.Width - shougouSize.Width) / 2), 151);
        using Image shougouImage = HeavyFont.DrawImage(14, shougou, Brushes.Solid(new Rgb24(255, 255, 255)),
            Pens.Solid(new Rgb24(0, 0, 0), 4f), [SymbolsFont, Symbols2Font, NotoBlackFont], KnownResamplers.Spline, 6);

        using Image nameImage = MediumFont.DrawImage(21, user.Name, new Color(new Rgb24(0, 0, 0)),
            [SymbolsFont, Symbols2Font, NotoMediumFont], KnownResamplers.Lanczos3);

        string scorePart1 = everTotal.ToString();
        string scorePart2 = "B35";
        string scorePart3 = $"+{currentTotal}";
        string scorePart4 = "B15";
        string scorePart5 = $"={everTotal + currentTotal}";
        FontRectangle scorePart1Size = BoldFont.GetSize(27, scorePart1, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        FontRectangle scorePart2Size = BoldFont.GetSize(19, scorePart2, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        FontRectangle scorePart3Size = BoldFont.GetSize(27, scorePart3, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        FontRectangle scorePart4Size = BoldFont.GetSize(19, scorePart4, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        FontRectangle scorePart5Size = BoldFont.GetSize(27, scorePart5, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        float scoreWidth = scorePart1Size.Width + scorePart2Size.Width + scorePart3Size.Width + scorePart4Size.Width +
                           scorePart5Size.Width;
        PointF scorePart1Pos = new(285 - (scoreWidth / 2), 532);
        PointF scorePart2Pos = new(scorePart1Pos.X + scorePart1Size.Width, 543);
        PointF scorePart3Pos = new(scorePart2Pos.X + scorePart2Size.Width, 532);
        PointF scorePart4Pos = new(scorePart3Pos.X + scorePart3Size.Width, 543);
        PointF scorePart5Pos = new(scorePart4Pos.X + scorePart4Size.Width, 532);
        Rgb24 scoreColorValue = new(75, 77, 138);
        Color scoreColor = new(scoreColorValue);
        using Image scorePart1Image = BoldFont.DrawImage(27, scorePart1, scoreColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart2Image = BoldFont.DrawImage(21, scorePart2, scoreColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart3Image = BoldFont.DrawImage(27, scorePart3, scoreColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart4Image = BoldFont.DrawImage(21, scorePart4, scoreColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart5Image = BoldFont.DrawImage(27, scorePart5, scoreColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);

        FontRectangle typeSize = BoldFont.GetSize(32, typename, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        PointF typePos = new(720 - (typeSize.Width / 2), 725);
        using Image typeImage = BoldFont.DrawImage(32, typename, new Color(new Rgb24(0, 109, 103)),
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);

        image.Mutate(ctx =>
        {
            ctx.DrawImage(frameImage, new Point(48, 45), 1);
            ctx.DrawImage(plate, new Point(77, 69), 1);
            ctx.DrawImage(namebase, new Point(183, 108), 1);
            ctx.DrawImage(ratingbase, new Point(181, 72), 1);
            ctx.DrawImage(shougoubase, shougoubasePos, 1);
            ctx.DrawImage(iconImage, new Point(86, 76), 1);
            ctx.DrawImage(ratingImage, new Point(264, 82), 1);
            ctx.DrawImage(@class, new Point(343, 48), 1);
            ctx.DrawImage(course, new Point(358, 109), 1);
            ctx.DrawImage(nameImage, new Point(190, 117), 1);
            ctx.DrawImage(shougouImage, (Point)shougouPos, 1);
            ctx.DrawImage(frameLine, new Point(40, 36), 1);
            ctx.DrawImage(scorebase, new Point(25, 492), 1);
            ctx.DrawImage(scorePart1Image, (Point)scorePart1Pos, 1);
            ctx.DrawImage(scorePart2Image, (Point)scorePart2Pos, 1);
            ctx.DrawImage(scorePart3Image, (Point)scorePart3Pos, 1);
            ctx.DrawImage(scorePart4Image, (Point)scorePart4Pos, 1);
            ctx.DrawImage(scorePart5Image, (Point)scorePart5Pos, 1);
            ctx.DrawImage(typeImage, (Point)typePos, 1);
            ctx.DrawImage(sdBests, new Point(25, 796), 1);
            ctx.DrawImage(dxBests, new Point(25, 1986), 1);
        });

        Image bg = Image.Load(backgroundPath);
        bg.Mutate(ctx => ctx.DrawImage(image, new Point(0, 0), 1));

        return bg;
    }

    public Image DrawScores(IList<CommonRecord> scores, int start_index = 0)
    {
        int count = scores.Count;
        int height = ((int)Math.Ceiling(((double)count + 1) / 4) * 120) - 10;
        Point point = new(350, 0);
        Image<Rgba32> image = new(1390, height);
        for (int index = 0; index < count;)
        {
            for (int rowIndex = 0, rowMaxIndex = point.Y == 0 ? 3 : 4;
                 rowIndex < rowMaxIndex && index < count;
                 ++index, ++rowIndex)
            {
                CommonRecord record = scores[index];
                int realIndex = index + start_index + 1;

                using Image part = DrawScore(record, realIndex);
                part.Resize(0.34, KnownResamplers.Lanczos3);
                image.Mutate(ctx => ctx.DrawImage(part, point, 1));

                point.X += 350;
            }

            point.X = 0;
            point.Y += 120;
        }

        return image;
    }

    public Image DrawScore(CommonRecord score, int index)
    {
        Rgb24 colorValue = new(255, 255, 255);
        if (score.Difficulty is CommonDifficulties.ReMaster)
        {
            colorValue = new(88, 140, 204);
        }

        Color color = new(colorValue);

        Image bg = Image.Load(Path.Combine(PartRootPath, $"{score.Difficulty}.png"));
        using Image jacket = Image.Load(Path.Combine(JacketRootPath, $"{score.Id % 10000}.png"));
        using Image songType = Image.Load(Path.Combine(SongTypeRootPath, $"{score.Type}.png"));
        using Image rank = Image.Load(Path.Combine(RateRootPath, $"{score.Rank}.png"));

        #region Title

        Font boldFont40 = BoldFont.GetSizeFont(40);

        string title = score.Title;
        string drawName = title;
        for (FontRectangle size = TextMeasurer.MeasureSize(drawName, new(boldFont40));
             size.Width > 450;
             size = TextMeasurer.MeasureSize(drawName, new(boldFont40)))
        {
            title = title[..^1];
            drawName = $"{title}…";
        }

        using Image titleImage = BoldFont.DrawImage(40, drawName, color, [SymbolsFont, Symbols2Font, NotoBoldFont],
            KnownResamplers.Lanczos3);

        #endregion

        #region Achievements

        string[] achievements = score.Achievements.ToString().Split('.');
        string achiPart1 = achievements[0];
        string achiPart2 = ".";
        string achiPart3 = achievements.Length > 1 ? achievements[1].PadRight(4, '0') : "0000";
        string achiPart4 = "%";

        FontRectangle achiPart1Size = HeavyFont.GetSize(76, achiPart1, [SymbolsFont, Symbols2Font, NotoBlackFont]);
        FontRectangle achiPart2Size = BoldFont.GetSize(76, achiPart2, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        FontRectangle achiPart3Size = HeavyFont.GetSize(54, achiPart3, [SymbolsFont, Symbols2Font, NotoBlackFont]);

        Point achiPart1Pos = new(371, 90);
        PointF achiPart2Pos = new(achiPart1Pos.X + achiPart1Size.Width, 90);
        PointF achiPart3Pos = new(achiPart2Pos.X + achiPart2Size.Width, 108);
        PointF achiPart4Pos = new(achiPart3Pos.X + achiPart3Size.Width, 100);

        using Image achiPart1Image = HeavyFont.DrawImage(76, achiPart1, color,
            [SymbolsFont, Symbols2Font, NotoBlackFont], KnownResamplers.Lanczos3);
        using Image achiPart2Image = BoldFont.DrawImage(76, achiPart2, color, [SymbolsFont, Symbols2Font, NotoBoldFont],
            KnownResamplers.Lanczos3);
        using Image achiPart3Image = HeavyFont.DrawImage(54, achiPart3, color,
            [SymbolsFont, Symbols2Font, NotoBlackFont], KnownResamplers.Lanczos3);
        using Image achiPart4Image = BoldFont.DrawImage(65, achiPart4, color, [SymbolsFont, Symbols2Font, NotoBoldFont],
            KnownResamplers.Lanczos3);

        #endregion

        #region Serial Number

        string indexPart1 = "#";
        string indexPart2 = index.ToString();
        FontRectangle indexPart1Size = BoldFont.GetSize(24, indexPart1, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        FontRectangle indexPart2Size = BoldFont.GetSize(30, indexPart2, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        float indexWidth = indexPart1Size.Width + indexPart2Size.Width;
        PointF indexPart1Pos = new(335 - (indexWidth / 2), 250);
        PointF indexPart2Pos = new(indexPart1Pos.X + indexPart1Size.Width, 245);
        Rgb24 indexColorValue = new(255, 255, 255);
        Color indexColor = new(indexColorValue);
        using Image indexPart1Image = BoldFont.DrawImage(24, indexPart1, indexColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
        using Image indexPart2Image = BoldFont.DrawImage(30, indexPart2, indexColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);

        #endregion

        #region LevelValue

        string[] level = score.LevelValue.ToString().Split('.');
        string levelPart1 = $"{level[0]}.";
        string levelPart2 = level.Length > 1 ? level[1] : "0";
        FontRectangle levelPart1Size = BoldFont.GetSize(34, levelPart1, [SymbolsFont, Symbols2Font, NotoBoldFont]);

        Point levelPart1Pos = new(375, 182);
        PointF levelPart2Pos = new(levelPart1Pos.X + levelPart1Size.Width, 187);
        using Image levelPart1Image = BoldFont.DrawImage(34, levelPart1, color,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
        using Image levelPart2Image = BoldFont.DrawImage(28, levelPart2, color,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);

        #endregion

        #region Rating

        string rating = score.DXRating.ToString();
        FontRectangle ratingSize = BoldFont.GetSize(34, rating, [SymbolsFont, Symbols2Font, NotoBoldFont]);

        PointF ratingPos = new(548 - ratingSize.Width, 182);
        using Image ratingImage = BoldFont.DrawImage(34, rating, color, [SymbolsFont, Symbols2Font, NotoBoldFont],
            KnownResamplers.Lanczos3);

        #endregion

        #region Numero

        string numeroPart1 = "No.";
        string numeroPart2 = score.Id.ToString();

        FontRectangle numeroPart1Size = BoldFont.GetSize(24, numeroPart1, [SymbolsFont, Symbols2Font, NotoBoldFont]);

        Point numeroPart1Pos = new(386, 250);
        PointF numeroPart2Pos = new(numeroPart1Pos.X + numeroPart1Size.Width, 245);

        Rgb24 numeroColorValue = new(28, 43, 120);
        Color numeroColor = new(numeroColorValue);
        using Image numeroPart1Image = BoldFont.DrawImage(24, numeroPart1, numeroColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont],
            KnownResamplers.Lanczos3);
        using Image numeroPart2Image = BoldFont.DrawImage(30, numeroPart2, numeroColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont],
            KnownResamplers.Lanczos3);

        #endregion

        #region DXScore

        string dxScorePart1 = $"{score.DXScore}/";
        string dxScorePart2 = score.TotalDXScore.ToString();

        FontRectangle dxScorePart1Size = BoldFont.GetSize(30, dxScorePart1, [SymbolsFont, Symbols2Font, NotoBoldFont]);
        FontRectangle dxScorePart2Size = BoldFont.GetSize(24, dxScorePart2, [SymbolsFont, Symbols2Font, NotoBoldFont]);

        PointF dxScorePart2Pos = new(734 - dxScorePart2Size.Width, 250);
        PointF dxScorePart1Pos = new(dxScorePart2Pos.X - dxScorePart1Size.Width, 245);
        using Image dxScorePart1Image = BoldFont.DrawImage(30, dxScorePart1, numeroColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
        using Image dxScorePart2Image = BoldFont.DrawImage(24, dxScorePart2, numeroColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);

        #endregion

        #region DXStar

        if (score.DXStar > 0)
        {
            int starIndex = score.DXStar switch
            {
                < 3 => 1,
                < 5 => 2,
                >= 5 => 3
            };

            PointF dxStarPos = new(570, 177);
            using Image dxStar = Image.Load(Path.Combine(DxStarRootPath, $"{starIndex}.png"));
            dxStar.Resize(1.3, KnownResamplers.Lanczos3);

            bg.Mutate(ctx =>
            {
                for (int i = 0; i < score.DXStar; ++i)
                {
                    ctx.DrawImage(dxStar, (Point)dxStarPos, 1);
                    dxStarPos.X += 31.2f;
                }
            });
        }

        #endregion

        jacket.Resize(0.56, KnownResamplers.Lanczos3);
        rank.Resize(0.88, KnownResamplers.Lanczos3);

        bg.Mutate(ctx =>
        {
            ctx.DrawImage(jacket, new Point(36, 40), 1);
            ctx.DrawImage(songType, new Point(775, 12), 1);
            ctx.DrawImage(rank, new Point(762, 78), 1);

            ctx.DrawImage(titleImage, new Point(295, 25), 1);

            ctx.DrawImage(achiPart1Image, achiPart1Pos, 1);
            ctx.DrawImage(achiPart2Image, (Point)achiPart2Pos, 1);
            ctx.DrawImage(achiPart3Image, (Point)achiPart3Pos, 1);
            ctx.DrawImage(achiPart4Image, (Point)achiPart4Pos, 1);

            ctx.DrawImage(indexPart1Image, (Point)indexPart1Pos, 1);
            ctx.DrawImage(indexPart2Image, (Point)indexPart2Pos, 1);

            ctx.DrawImage(levelPart1Image, levelPart1Pos, 1);
            ctx.DrawImage(levelPart2Image, (Point)levelPart2Pos, 1);

            ctx.DrawImage(ratingImage, (Point)ratingPos, 1);

            ctx.DrawImage(numeroPart1Image, numeroPart1Pos, 1);
            ctx.DrawImage(numeroPart2Image, (Point)numeroPart2Pos, 1);

            ctx.DrawImage(dxScorePart1Image, (Point)dxScorePart1Pos, 1);
            ctx.DrawImage(dxScorePart2Image, (Point)dxScorePart2Pos, 1);
        });

        if (score.ComboFlag > ComboFlags.None)
        {
            using Image combo = Image.Load(Path.Combine(ComboRootPath, $"{score.ComboFlag}_S.png"));
            combo.Resize(1.2623, KnownResamplers.Lanczos3);
            bg.Mutate(ctx => ctx.DrawImage(combo, new Point(779, 188), 1));
        }

        if (score.SyncFlag > SyncFlags.None)
        {
            using Image sync = Image.Load(Path.Combine(SyncRootPath, $"{score.SyncFlag}_S.png"));
            sync.Resize(1.2623, KnownResamplers.Lanczos3);
            bg.Mutate(ctx => ctx.DrawImage(sync, new Point(873, 188), 1));
        }

        return bg;
    }
}