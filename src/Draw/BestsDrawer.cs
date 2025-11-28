using Limekuma.Prober.Common;
using Limekuma.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace Limekuma.Draw;

public class BestsDrawer : DrawerBase
{
#if RELEASE
    public const string IconRootPath = "./Cache/Icon/";
    public const string PlateRootPath = "./Cache/Plate/";
    public const string FrameRootPath = "./Cache/Frame/";
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
    public const string BackgroundAnimationPath = "./Static/Maimai/Bests/background_animation.png";
    public const string LevelSugBackgroundPath = "./Static/Maimai/Bests/rating_base.png";
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
    public const string BackgroundAnimationPath = "./Resources/Background/bests_animation.png";
    public const string LevelSugBackgroundPath = "./Resources/Background/level_seg.png";
#endif

    public Image Draw(CommonUser user, IReadOnlyList<CommonRecord> ever, IReadOnlyList<CommonRecord> current, int everTotal,
        int currentTotal, string typename, string prober) => Draw(user, ever, current, everTotal, currentTotal,
        typename, prober, false, false);

    public Image Draw(CommonUser user, IReadOnlyList<CommonRecord> ever, IReadOnlyList<CommonRecord> current, int everTotal,
        int currentTotal, string typename, string prober, bool isAnime) => Draw(user, ever, current, everTotal,
        currentTotal, typename, prober, isAnime, false);

    public Image Draw(CommonUser user, IReadOnlyList<CommonRecord> ever, IReadOnlyList<CommonRecord> current, int everTotal,
        int currentTotal, string typename, string prober, bool isAnime, bool drawLevelSeg)
    {
        Image bg = AssetManager.Shared.Load(isAnime ? BackgroundAnimationPath : BackgroundPath);
        List<(Point, Image)> sdBests = DrawScores(ever, isAnime: isAnime);
        List<(Point, Image)> dxBests = DrawScores(current, ever.Count, isAnime);
        using Image frameImage = AssetManager.Shared.Load(Path.Combine(FrameRootPath, $"{user.FrameId}.png"));
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

        using Image nameImage = MediumFont.DrawImage(21, user.Name, new(new Rgb24(0, 0, 0)),
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
        using Image typeImage = BoldFont.DrawImage(32, typename, new(new Rgb24(0, 109, 103)),
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);

        if (isAnime)
        {
            using Image scorebase = AssetManager.Shared.Load(ScorebasePath);
            bg.Mutate(ctx => ctx.DrawImage(scorebase, new Point(25, 492), 1));
        }

        bg.Mutate(ctx =>
        {
            ctx.DrawImage(frameImage, new Point(48, 45), 1);
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
            ctx.DrawImage(frameLine, new Point(40, 36), 1);
            ctx.DrawImage(scorePart1Image, (Point)scorePart1Pos, 1);
            ctx.DrawImage(scorePart2Image, (Point)scorePart2Pos, 1);
            ctx.DrawImage(scorePart3Image, (Point)scorePart3Pos, 1);
            ctx.DrawImage(scorePart4Image, (Point)scorePart4Pos, 1);
            ctx.DrawImage(scorePart5Image, (Point)scorePart5Pos, 1);
            ctx.DrawImage(typeImage, (Point)typePos, 1);
            ctx.DrawImage(proberLogo, new Point(1011, 67), 1);
            foreach ((Point point, Image scoreImage) in sdBests)
            {
                using (scoreImage)
                {
                    Point realPoint = point;
                    realPoint.X += 25;
                    realPoint.Y += 796;
                    GraphicsOptions options = ctx.GetGraphicsOptions();
                    DrawImageProcessor processor = new(scoreImage, realPoint, scoreImage.Bounds,
                        options.ColorBlendingMode, options.AlphaCompositionMode, 1);
                    using (IImageProcessor<Rgba32> specificProcessor =
                           processor.CreatePixelSpecificProcessor(ctx.Configuration, (Image<Rgba32>)bg, bg.Bounds))
                    {
                        specificProcessor.Execute();
                    }

                    ctx.ApplyProcessor(processor);
                }
            }

            foreach ((Point point, Image scoreImage) in dxBests)
            {
                using (scoreImage)
                {
                    Point realPoint = point;
                    realPoint.X += 25;
                    realPoint.Y += 1986;
                    GraphicsOptions options = ctx.GetGraphicsOptions();
                    DrawImageProcessor processor = new(scoreImage, realPoint, scoreImage.Bounds,
                        options.ColorBlendingMode, options.AlphaCompositionMode, 1);
                    using (IImageProcessor<Rgba32> specificProcessor =
                           processor.CreatePixelSpecificProcessor(ctx.Configuration, (Image<Rgba32>)bg, bg.Bounds))
                    {
                        specificProcessor.Execute();
                    }

                    ctx.ApplyProcessor(processor);
                }
            }
        });
        if (drawLevelSeg)
        {
            using Image levelSegImage = DrawLevelSug(ever, current);
            bg.Mutate(ctx => ctx.DrawImage(levelSegImage, new Point(60, 197), 1));
        }

        return bg;
    }

    public List<(Point, Image)> DrawScores(IReadOnlyList<CommonRecord> scores, int start_index = 0, bool isAnime = false)
    {
        int count = scores.Count;
        if (count is 0)
        {
            return [];
        }

        (Point, Image)[] results = new (Point, Image)[count];

        Parallel.For(0, count, idx =>
        {
            CommonRecord record = scores[idx];
            int realIndex = idx + start_index + 1;

            Point p;
            if (idx < 3)
            {
                p = new(350 + (idx * 350), 0);
            }
            else
            {
                int remain = idx - 3;
                int row = remain / 4;
                int col = remain % 4;
                p = new(col * 350, (row + 1) * 120);
            }

            Image part = DrawScore(record, realIndex,
                isAnime && record.Rank is Ranks.SSSPlus &&
                record.Difficulty is CommonDifficulties.Master or CommonDifficulties.ReMaster &&
                record.DXRating >= 315);
            part.Resize(0.34, KnownResamplers.Lanczos3);
            results[idx] = (p, part);
        });

        return [.. results];
    }

    public Image DrawScore(CommonRecord score, int index, bool isMax = false)
    {
        Rgb24 colorValue = new(255, 255, 255);
        if (score.Difficulty is CommonDifficulties.ReMaster && !isMax)
        {
            colorValue = new(88, 140, 204);
        }

        Color color = new(colorValue);
        string bg_filename = $"{score.Difficulty}.png";
        if (isMax)
        {
            bg_filename = $"{score.Difficulty}_max.png";
        }

        Image bg = AssetManager.Shared.Load(Path.Combine(PartRootPath, bg_filename));
        using Image jacket = AssetManager.Shared.Load(Path.Combine(JacketRootPath, $"{score.Id % 10000}.png"));
        using Image songType = AssetManager.Shared.Load(Path.Combine(SongTypeRootPath, $"{score.Type}.png"));
        using Image rank = AssetManager.Shared.Load(Path.Combine(RateRootPath, $"{score.Rank}.png"));

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
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
        using Image numeroPart2Image = BoldFont.DrawImage(30, numeroPart2, numeroColor,
            [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);

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
            using Image dxStar = AssetManager.Shared.Load(Path.Combine(DxStarRootPath, $"{starIndex}.png"));
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
            using Image combo = AssetManager.Shared.Load(Path.Combine(ComboRootPath, $"{score.ComboFlag}_S.png"));
            combo.Resize(1.2623, KnownResamplers.Lanczos3);
            bg.Mutate(ctx => ctx.DrawImage(combo, new Point(779, 188), 1));
        }

        if (score.SyncFlag > SyncFlags.None)
        {
            using Image sync = AssetManager.Shared.Load(Path.Combine(SyncRootPath, $"{score.SyncFlag}_S.png"));
            sync.Resize(1.2623, KnownResamplers.Lanczos3);
            bg.Mutate(ctx => ctx.DrawImage(sync, new Point(873, 188), 1));
        }

        if (!isMax)
        {
            return bg;
        }

        using Image jacketMask = AssetManager.Shared.Load(Path.Combine(JacketRootPath, "mask.png"));
        using Image rankMask = AssetManager.Shared.Load(Path.Combine(RateRootPath, "mask.png"));
        bg.Mutate(ctx =>
        {
            GraphicsOptions options = ctx.GetGraphicsOptions();
            DrawImageProcessor jacketMaskProcessor = new(jacketMask, new(13, 17), jacketMask.Bounds,
                options.ColorBlendingMode, options.AlphaCompositionMode, 1);
            using (IImageProcessor<Rgba32> jacketMaskspecificProcessor =
                   jacketMaskProcessor.CreatePixelSpecificProcessor(ctx.Configuration, (Image<Rgba32>)bg, bg.Bounds))
            {
                jacketMaskspecificProcessor.Execute();
            }

            ctx.ApplyProcessor(jacketMaskProcessor);
            DrawImageProcessor rankMaskProcessor = new(rankMask, new(790, 78), rankMask.Bounds,
                options.ColorBlendingMode, options.AlphaCompositionMode, 1);
            using (IImageProcessor<Rgba32> rankMaskspecificProcessor =
                   rankMaskProcessor.CreatePixelSpecificProcessor(ctx.Configuration, (Image<Rgba32>)bg, bg.Bounds))
            {
                rankMaskspecificProcessor.Execute();
            }

            ctx.ApplyProcessor(rankMaskProcessor);
        });
        if (score.ComboFlag > ComboFlags.None || score.SyncFlag > SyncFlags.None)
        {
            using Image comboMask = AssetManager.Shared.Load(Path.Combine(ComboRootPath, "mask.png"));
            if (score.ComboFlag > ComboFlags.None)
            {
                bg.Mutate(ctx =>
                {
                    GraphicsOptions options = ctx.GetGraphicsOptions();
                    DrawImageProcessor processor = new(comboMask, new(774, 189), comboMask.Bounds,
                        PixelColorBlendingMode.Screen, options.AlphaCompositionMode, 1);
                    using (IImageProcessor<Rgba32> specificProcessor =
                           processor.CreatePixelSpecificProcessor(ctx.Configuration, (Image<Rgba32>)bg, bg.Bounds))
                    {
                        specificProcessor.Execute();
                    }

                    ctx.ApplyProcessor(processor);
                });
            }

            if (score.SyncFlag > SyncFlags.None)
            {
                bg.Mutate(ctx =>
                {
                    GraphicsOptions options = ctx.GetGraphicsOptions();
                    DrawImageProcessor processor = new(comboMask, new(868, 189), comboMask.Bounds,
                        PixelColorBlendingMode.Screen, options.AlphaCompositionMode, 1);
                    using (IImageProcessor<Rgba32> specificProcessor =
                           processor.CreatePixelSpecificProcessor(ctx.Configuration, (Image<Rgba32>)bg, bg.Bounds))
                    {
                        specificProcessor.Execute();
                    }

                    ctx.ApplyProcessor(processor);
                });
            }
        }

        return bg;
    }

    public Image DrawLevelSug(IReadOnlyList<CommonRecord> ever, IReadOnlyList<CommonRecord> current)
    {
        Image bg = AssetManager.Shared.Load(LevelSugBackgroundPath);
        (Point, Image)[] images = new (Point, Image)[20];
        int b35max = 0;
        int b35min = 0;
        if (ever.Count > 0)
        {
            b35max = ever[0].DXRating;
            b35min = ever[^1].DXRating;
        }

        int b35maxDiff = b35max;
        if (ever.Count > 34)
        {
            b35maxDiff -= b35min;
        }

        int b35minDiff = b35maxDiff > 0 ? 1 : 0;
        int b15max = 0;
        int b15min = 0;
        if (current.Count > 0)
        {
            b15max = current[0].DXRating;
            b15min = current[^1].DXRating;
        }

        int b15maxDiff = b15max;
        if (current.Count > 14)
        {
            b15maxDiff -= b15min;
        }

        int b15minDiff = b15maxDiff > 0 ? 1 : 0;
        Parallel.For(0, 4, i =>
        {
            ReadOnlySpan<int> posY = [73, 113, 179, 219];
            ReadOnlySpan<int> diffs = [b35maxDiff, b35minDiff, b15maxDiff, b15minDiff];
            Point pos = new(140, posY[i % 4]);
            Image levelImage = BoldFont.DrawImage(30, $"{$"+{diffs[i]}",4}", new(new Rgb24(255, 255, 255)),
                [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
            images[i] = (pos, levelImage);
        });
        Parallel.For(0, 16, i =>
        {
            ReadOnlySpan<int> posX = [270, 390, 510, 630];
            ReadOnlySpan<int> posY = [73, 113, 179, 219];
            ReadOnlySpan<Ranks> ranks = [Ranks.SSSPlus, Ranks.SSS, Ranks.SSPlus, Ranks.SS];
            ReadOnlySpan<int> ratings = [b35max, b35min, b15max, b15min];
            int indexX = i % 4;
            int indexY = i / 4;
            int rating = ratings[indexY];
            Ranks rank = ranks[indexX];
            double currentRating = RatingProc(rating, rank);
            string ratingText = currentRating > 0 ? $"{currentRating,4:F1}" : "-----";
            Point pos = new(posX[indexX], posY[indexY]);
            Image levelImage = BoldFont.DrawImage(30, ratingText, new(new Rgb24(255, 255, 255)),
                [SymbolsFont, Symbols2Font, NotoBoldFont], KnownResamplers.Lanczos3);
            images[i + 4] = (pos, levelImage);
        });
        bg.Mutate(ctx =>
        {
            foreach ((Point point, Image image) in images)
            {
                using (image)
                {
                    ctx.DrawImage(image, point, 1);
                }
            }
        });
        return bg;
    }

    private readonly Dictionary<Ranks, (double achievements, double rating, double ratingOld)> _ratings = new()
    {
        { Ranks.SSSPlus, (100.5, 0.224, 15) },
        { Ranks.SSS, (100, 0.216, 14) },
        { Ranks.SSPlus, (99.5, 0.211, 13) },
        { Ranks.SS, (99, 0.208, 12) },
        { Ranks.SPlus, (98, 0.203, 11) },
        { Ranks.S, (97, 0.2, 10) },
        { Ranks.AAA, (94, 0.168, 9.4) },
        { Ranks.AA, (90, 0.152, 9) },
        { Ranks.A, (80, 0.136, 8) },
        { Ranks.BBB, (75, 0.12, 7.5) },
        { Ranks.BB, (70, 0.112, 7) },
        { Ranks.B, (60, 0.096, 6) },
        { Ranks.C, (50, 0.08, 5) },
        { Ranks.D, (0.0001, 0.064, 4) }
    };

    private double RatingProc(int rating, Ranks rank)
    {
        if (rating < 232)
        {
            return 0;
        }

        if (!_ratings.TryGetValue(rank, out (double, double, double) ratings))
        {
            return 0;
        }

        (double achievements, double currentRating, _) = ratings;
        if (achievements is 0 || currentRating is 0)
        {
            return 0;
        }

        double result = rating / (achievements * currentRating);
        if (result > 15)
        {
            return 0;
        }

        return result;
    }
}
