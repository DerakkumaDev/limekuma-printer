using DXKumaBot.Backend.Prober.Common;
using DXKumaBot.Backend.Prober.Lxns;
using DXKumaBot.Backend.Prober.Lxns.Models;
using DXKumaBot.Backend.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static DXKumaBot.Backend.Utils.Shared;

namespace DXKumaBot.Backend;

public class Draw
{
    #region Font Families
    // Main fonts (Rounded-X M+ 1p from 自家製フォント工房)
    private static readonly FontCollection fonts = new();
    private static readonly FontFamily mediumFont = fonts.Add(Path.Combine(FontRootPath, "rounded-x-mplus-1p-medium.ttf"));
    private static readonly FontFamily boldFont = fonts.Add(Path.Combine(FontRootPath, "rounded-x-mplus-1p-bold.ttf"));
    private static readonly FontFamily heavyFont = fonts.Add(Path.Combine(FontRootPath, "rounded-x-mplus-1p-heavy.ttf"));

    // Fallback fonts (思源黑体/Noto Sans from Adobe & Google)
    private static readonly FontFamily notoMediumFont = fonts.Add(Path.Combine(FontRootPath, "NotoSansCJKsc-Medium.otf"));
    private static readonly FontFamily notoBoldFont = fonts.Add(Path.Combine(FontRootPath, "NotoSansCJKsc-Bold.otf"));
    private static readonly FontFamily notoBlackFont = fonts.Add(Path.Combine(FontRootPath, "NotoSansCJKsc-Black.otf"));
    private static readonly FontFamily symbolsFont = fonts.Add(Path.Combine(FontRootPath, "NotoSansSymbols-Regular.ttf"));
    private static readonly FontFamily symbols2Font = fonts.Add(Path.Combine(FontRootPath, "NotoSansSymbols2-Regular.ttf"));
    #endregion

    private static readonly LxnsResourceClient resource = new();
    private static readonly SongList songList = resource.GetSongListAsync(includeNotes: true).Result;

    public Image DrawBests(CommonUser user, List<CommonRecord> ever, List<CommonRecord> current, int everTotal, int currentTotal, string backgroundPath = BackgroundPath)
    {
        using Image image = new Image<Rgba32>(1440, 2560, new(0, 0, 0, 0));
        using Image frameImage = Image.Load(Path.Combine(FrameRootPath, $"UI_Frame_{user.FrameId.ToString().PadLeft(6, '0')}.png"));
        using Image plate = Image.Load(Path.Combine(PlateRootPath, $"{user.PlateId.ToString().PadLeft(6, '0')}.png"));
        using Image iconImage = Image.Load(Path.Combine(IconRootPath, $"{user.IconId}.png"));
        using Image @class = Image.Load(Path.Combine(ClassRootPath, $"{(int)user.ClassRank}.png"));
        using Image course = Image.Load(Path.Combine(DaniRootPath, $"{(int)user.CourseRank}.png"));
        using Image shougoubase = Image.Load(Path.Combine(ShougouRootPath, $"{user.TrophyColor}.png"));
        using Image frameLine = Image.Load(FrameLinePath);
        using Image namebase = Image.Load(NamebasePath);
        using Image scorebase = Image.Load(ScorebasePath);
        using Image ratingbase = Image.Load(Path.Combine(RatingRootPath, $"{user.RatingLevel}.png"));

        List<char> ratingLE = [.. user.Rating.ToString().Reverse()];
        int[] ratingPos = [114, 86, 56, 28, 0];
        string shougou = $"{user.TrophyText}";

        using Image sdBests = DrawScores(ever);
        using Image dxBests = DrawScores(current, ever.Count);

        frameImage.Resize(0.95, KnownResamplers.Lanczos3);
        plate.Resize(0.95, KnownResamplers.Lanczos3);
        iconImage.Resize(0.75, KnownResamplers.Lanczos3);
        ratingbase.Resize(0.96, KnownResamplers.Lanczos3);
        @class.Resize(0.245, KnownResamplers.Lanczos3);
        course.Resize(0.213, KnownResamplers.Lanczos3);
        shougoubase.Resize(0.94, KnownResamplers.Lanczos3);
        frameLine.Resize(0.745, KnownResamplers.Lanczos3);

        Font notoBlackFont16 = notoBlackFont.GetSizeFont(32);
        Font heavyFont14 = heavyFont.GetSizeFont(14);

        using Image ratingImage = new Image<Rgba32>(512, 64);
        ratingImage.Mutate(ctx =>
        {
            for (int i = 0; i < ratingLE.Count; i++)
            {
                ctx.DrawText(ratingLE[i].ToString(), notoBlackFont16, new Color(new Rgb24(249, 198, 10)), new(ratingPos[i], 0));
            }

            ctx.Resize(ratingImage.Width * 16 / 32, ratingImage.Height * 16 / 32, KnownResamplers.Spline);
        });

        FontRectangle shougouSize = TextMeasurer.MeasureAdvance(shougou, new(heavyFont14)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        Point shougoubasePos = new(181, 143);
        PointF shougouPos = new(shougoubasePos.X + ((shougoubase.Width - shougouSize.Width) / 2), 151);

        using Image shougouImage = heavyFont.DrawImage(14, shougou, Brushes.Solid(new Rgb24(255, 255, 255)), Pens.Solid(new Rgb24(0, 0, 0), 2f), [symbolsFont, symbols2Font, notoBlackFont], KnownResamplers.Spline);
        using Image nameImage = mediumFont.DrawImage(22, user.Name, new Color(new Rgb24(0, 0, 0)), [symbolsFont, symbols2Font, notoMediumFont], KnownResamplers.Lanczos3);

        Font boldFont21 = boldFont.GetSizeFont(21);
        Font boldFont27 = boldFont.GetSizeFont(27);

        string scorePart1 = everTotal.ToString();
        string scorePart2 = "B35";
        string scorePart3 = "+";
        string scorePart4 = currentTotal.ToString();
        string scorePart5 = "B15";
        string scorePart6 = "=";
        string scorePart7 = (everTotal + currentTotal).ToString();
        FontRectangle scorePart1Size = TextMeasurer.MeasureAdvance(scorePart1, new(boldFont27)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        FontRectangle scorePart2Size = TextMeasurer.MeasureAdvance(scorePart2, new(boldFont21)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        FontRectangle scorePart3Size = TextMeasurer.MeasureAdvance(scorePart3, new(boldFont27)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        FontRectangle scorePart4Size = TextMeasurer.MeasureAdvance(scorePart4, new(boldFont27)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        FontRectangle scorePart5Size = TextMeasurer.MeasureAdvance(scorePart5, new(boldFont21)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        FontRectangle scorePart6Size = TextMeasurer.MeasureAdvance(scorePart6, new(boldFont27)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        FontRectangle scorePart7Size = TextMeasurer.MeasureAdvance(scorePart7, new(boldFont27)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        float scoreWidth = scorePart1Size.Width + scorePart2Size.Width + scorePart3Size.Width + scorePart4Size.Width + scorePart5Size.Width + scorePart6Size.Width + scorePart7Size.Width;
        PointF scorePart1Pos = new(285 - (scoreWidth / 2), 532);
        PointF scorePart2Pos = new(scorePart1Pos.X + scorePart1Size.Width, 543);
        PointF scorePart3Pos = new(scorePart2Pos.X + scorePart2Size.Width, 532);
        PointF scorePart4Pos = new(scorePart3Pos.X + scorePart3Size.Width, 532);
        PointF scorePart5Pos = new(scorePart4Pos.X + scorePart4Size.Width, 543);
        PointF scorePart6Pos = new(scorePart5Pos.X + scorePart5Size.Width, 532);
        PointF scorePart7Pos = new(scorePart6Pos.X + scorePart6Size.Width, 532);
        Rgb24 scoreColorValue = new(75, 77, 138);
        Color scoreColor = new(scoreColorValue);
        using Image scorePart1Image = boldFont.DrawImage(27, scorePart1, scoreColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart2Image = boldFont.DrawImage(21, scorePart2, scoreColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart3Image = boldFont.DrawImage(27, scorePart3, scoreColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart4Image = boldFont.DrawImage(27, scorePart4, scoreColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart5Image = boldFont.DrawImage(21, scorePart5, scoreColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart6Image = boldFont.DrawImage(27, scorePart6, scoreColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image scorePart7Image = boldFont.DrawImage(27, scorePart7, scoreColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);

        image.Mutate(ctx =>
        {
            ctx.DrawImage(frameImage, new Point(48, 45), 1);
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
            ctx.DrawImage(frameLine, new Point(40, 36), 1);
            ctx.DrawImage(scorebase, new Point(25, 492), 1);
            ctx.DrawImage(scorePart1Image, (Point)scorePart1Pos, 1);
            ctx.DrawImage(scorePart2Image, (Point)scorePart2Pos, 1);
            ctx.DrawImage(scorePart3Image, (Point)scorePart3Pos, 1);
            ctx.DrawImage(scorePart4Image, (Point)scorePart4Pos, 1);
            ctx.DrawImage(scorePart5Image, (Point)scorePart5Pos, 1);
            ctx.DrawImage(scorePart6Image, (Point)scorePart6Pos, 1);
            ctx.DrawImage(scorePart7Image, (Point)scorePart7Pos, 1);
            ctx.DrawImage(sdBests, new Point(25, 795), 1);
            ctx.DrawImage(dxBests, new Point(25, 1985), 1);
        });

        Image bg = Image.Load(backgroundPath);
        bg.Mutate(ctx => ctx.DrawImage(image, new Point(0, 0), 1));

        return bg;
    }
    public Image DrawScores(List<CommonRecord> scores, int start_index = 0)
    {
        int index = 0;
        int count = scores.Count;
        int height = 110;
        if (count > 3)
        {
            height = Convert.ToInt32(Math.Ceiling(((((Convert.ToDecimal(count) - 3) / 4) + 1) * 120) - 10));
        }

        Point point = new(350, 0);
        Image image = new Image<Rgba32>(1390, height, new(0, 0, 0, 0));
        for (int columnIndex = 0; index < count; point.X = 0, point.Y += 120, ++columnIndex)
        {
            int rowMaxIndex = 3;
            if (columnIndex > 0)
            {
                rowMaxIndex = 4;
            }

            for (int rowIndex = 0; rowIndex < rowMaxIndex; point.X += 350, ++index, ++rowIndex)
            {
                CommonRecord record = index < count ? scores[index] : new()
                {
                    Id = 0,
                    Title = string.Empty,
                    Difficulty = CommonDifficulties.Dummy,
                    Type = CommonSongTypes.Standard,
                    Achievements = 0,
                    DXScore = 0,
                    ComboFlag = ComboFlags.None,
                    SyncFlag = SyncFlags.None,
                    Rank = Ranks.D,
                    DXRating = 0
                };
                int realIndex = index + start_index + 1;
                using Image part = DrawScore(record, realIndex);
                part.Resize(0.34, KnownResamplers.Lanczos3);
                image.Mutate(ctx => ctx.DrawImage(part, point, 1));
            }
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

        #region Fonts
        Font boldFont20 = boldFont.GetSizeFont(20);
        Font boldFont24 = boldFont.GetSizeFont(24);
        Font boldFont28 = boldFont.GetSizeFont(28);
        Font boldFont30 = boldFont.GetSizeFont(30);
        Font boldFont34 = boldFont.GetSizeFont(34);
        Font boldFont40 = boldFont.GetSizeFont(40);
        Font boldFont76 = boldFont.GetSizeFont(76);
        Font heavyFont54 = heavyFont.GetSizeFont(54);
        Font heavyFont65 = heavyFont.GetSizeFont(65);
        Font heavyFont76 = heavyFont.GetSizeFont(76);
        #endregion

        #region Title
        string title = score.Title;
        string drawName = title;
        for (FontRectangle size = TextMeasurer.MeasureSize(drawName, new(boldFont40));
             size.Width > 450;
             title = title[0..^1],
             drawName = $"{title}…",
             size = TextMeasurer.MeasureSize(drawName, new(boldFont40))) { }

        using Image titleImage = boldFont.DrawImage(40, drawName, color, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        #endregion

        #region Achievements
        string[] achievements = score.Achievements.ToString().Split('.');
        string achiPart1 = achievements[0].ToString();
        string achiPart2 = ".";
        string achiPart3 = achievements.Length > 1 ? achievements[1].PadRight(4, '0') : "0000";
        string achiPart4 = "%";

        Font achiPart1Font = heavyFont76;
        Font achiPart2Font = boldFont76;
        Font achiPart3Font = heavyFont54;
        FontRectangle achiPart1Size = TextMeasurer.MeasureAdvance(achiPart1, new(achiPart1Font)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBlackFont]
        });
        FontRectangle achiPart2Size = TextMeasurer.MeasureAdvance(achiPart2, new(achiPart2Font)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        FontRectangle achiPart3Size = TextMeasurer.MeasureAdvance(achiPart3, new(achiPart3Font)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBlackFont]
        });

        Point achiPart1Pos = new(371, 90);
        PointF achiPart2Pos = new(achiPart1Pos.X + achiPart1Size.Width, 90);
        PointF achiPart3Pos = new(achiPart2Pos.X + achiPart2Size.Width, 108);
        PointF achiPart4Pos = new(achiPart3Pos.X + achiPart3Size.Width, 100);

        using Image achiPart1Image = heavyFont.DrawImage(76, achiPart1, color, [symbolsFont, symbols2Font, notoBlackFont], KnownResamplers.Lanczos3);
        using Image achiPart2Image = boldFont.DrawImage(76, achiPart2, color, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image achiPart3Image = heavyFont.DrawImage(54, achiPart3, color, [symbolsFont, symbols2Font, notoBlackFont], KnownResamplers.Lanczos3);
        using Image achiPart4Image = boldFont.DrawImage(65, achiPart4, color, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        #endregion

        #region Serial Number
        string indexPart1 = "#";
        string indexPart2 = index.ToString();
        FontRectangle indexPart1Size = TextMeasurer.MeasureAdvance(indexPart1, new(boldFont24)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        FontRectangle indexPart2Size = TextMeasurer.MeasureAdvance(indexPart2, new(boldFont30)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        float indexWidth = indexPart1Size.Width + indexPart2Size.Width;
        PointF indexPart1Pos = new(335 - (indexWidth / 2), 250);
        PointF indexPart2Pos = new(indexPart1Pos.X + indexPart1Size.Width, 245);
        Rgb24 indexColorValue = new(255, 255, 255);
        Color indexColor = new(indexColorValue);
        using Image indexPart1Image = boldFont.DrawImage(24, indexPart1, indexColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image indexPart2Image = boldFont.DrawImage(30, indexPart2, indexColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        #endregion

        #region LevelValue
        Song song = songList.Songs.First(x => x.Id == score.Id % 10000);
        SongDifficulty chart = (score.Type switch
        {
            CommonSongTypes.Standard => song.Difficulties.Standard,
            CommonSongTypes.DX => song.Difficulties.DX,
            _ => throw new InvalidDataException()
        })[((int)score.Difficulty) - 1];
        decimal levelValue = chart.LevelValue;

        string[] level = levelValue.ToString().Split('.');
        string levelPart1 = $"{level[0]}.";
        string levelPart2 = level.Length > 1 ? level[1] : "0";
        Font levelPart1Font = boldFont34;
        FontRectangle levelPart1Size = TextMeasurer.MeasureAdvance(levelPart1, new(levelPart1Font)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });

        Point levelPart1Pos = new(375, 182);
        PointF levelPart2Pos = new(levelPart1Pos.X + levelPart1Size.Width, 187);
        using Image levelPart1Image = boldFont.DrawImage(34, levelPart1, color, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image levelPart2Image = boldFont.DrawImage(28, levelPart2, color, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        #endregion

        #region Rating
        string rating = Math.Floor(score.DXRating).ToString();
        FontRectangle ratingSize = TextMeasurer.MeasureAdvance(rating, new(levelPart1Font)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });

        PointF ratingPos = new(548 - ratingSize.Width, 182);
        using Image ratingImage = boldFont.DrawImage(34, rating, color, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        #endregion

        #region ID
        string idPart1 = score.Id.ToString();
        string idPart2 = "ID";

        FontRectangle idPart1Size = TextMeasurer.MeasureAdvance(idPart1, new(boldFont30)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });

        Point idPart1Pos = new(386, 245);
        PointF idPart2Pos = new(idPart1Pos.X + idPart1Size.Width, 253);

        Rgb24 idColorValue = new(28, 43, 120);
        Color idColor = new(idColorValue);
        using Image idPart1Image = boldFont.DrawImage(30, idPart1, idColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image idPart2Image = boldFont.DrawImage(24, idPart2, idColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        #endregion

        #region DXScore
        int totalDXScore = chart.Notes!.Total * 3;
        string dxScorePart1 = $"{score.DXScore}/";
        string dxScorePart2 = totalDXScore.ToString();

        Font dxScorePart1Font = boldFont30;
        Font dxScorePart2Font = boldFont24;
        FontRectangle dxScorePart1Size = TextMeasurer.MeasureAdvance(dxScorePart1, new(dxScorePart1Font)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });
        FontRectangle dxScorePart2Size = TextMeasurer.MeasureAdvance(dxScorePart2, new(dxScorePart2Font)
        {
            FallbackFontFamilies = [symbolsFont, symbols2Font, notoBoldFont]
        });

        PointF dxScorePart2Pos = new(734 - dxScorePart2Size.Width, 250);
        PointF dxScorePart1Pos = new(dxScorePart2Pos.X - dxScorePart1Size.Width, 245);
        using Image dxScorePart1Image = boldFont.DrawImage(30, dxScorePart1, idColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        using Image dxScorePart2Image = boldFont.DrawImage(24, dxScorePart2, idColor, [symbolsFont, symbols2Font, notoBoldFont], KnownResamplers.Lanczos3);
        #endregion

        #region DXStar
        float dxScorePersent = (float)score.DXScore / totalDXScore;
        if (dxScorePersent >= 0.85)
        {
            (int stars, int starIndex) = dxScorePersent switch
            {
                < 0.90f => (1, 1),
                < 0.93f => (2, 1),
                < 0.95f => (3, 2),
                < 0.97f => (4, 2),
                <= 1 => (5, 3),
                _ => throw new InvalidDataException()
            };

            PointF dxStarPos = new(570, 177);
            using Image dxStar = Image.Load(Path.Combine(DxStarRootPath, $"{starIndex}.png"));
            dxStar.Resize(1.3, KnownResamplers.Lanczos3);

            bg.Mutate(ctx =>
            {
                for (int i = 0; i < stars; ++i)
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
            ctx.DrawImage(jacket, new Point(36, 41), 1);
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

            ctx.DrawImage(idPart1Image, idPart1Pos, 1);
            ctx.DrawImage(idPart2Image, (Point)idPart2Pos, 1);

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
