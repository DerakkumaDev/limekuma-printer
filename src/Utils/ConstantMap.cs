using Limekuma.Prober.Common;
using System.Collections.Frozen;

namespace Limekuma.Utils;

public static class ConstantMap
{
    private static readonly (float, float, float)[] RatingFactors =
    [
        (0, 0.64f, 4),
        (50, 0.8f, 5),
        (60, 0.96f, 6),
        (70, 0.112f, 7),
        (75, 0.12f, 7.5f),
        (80, 0.136f, 8),
        (90, 0.152f, 9),
        (94, 0.168f, 9.4f),
        (97, 0.2f, 10),
        (98, 0.203f, 11),
        (99, 0.208f, 12),
        (99.5f, 0.211f, 13),
        (100, 0.216f, 14),
        (100.5f, 0.224f, 15),
        (101, 0.224f, 15)
    ];

    public static readonly FrozenDictionary<string, FrozenSet<string>> VersionMap =
        new Dictionary<string, FrozenSet<string>>
        {
            ["真"] = ["maimai", "maimai PLUS"],
            ["超"] = ["maimai GreeN", "GreeN"],
            ["檄"] = ["maimai GreeN PLUS", "GreeN PLUS"],
            ["橙"] = ["maimai ORANGE", "ORANGE"],
            ["晓"] = ["maimai ORANGE PLUS", "ORANGE PLUS"],
            ["桃"] = ["maimai PiNK", "PiNK"],
            ["樱"] = ["maimai PiNK PLUS", "PiNK PLUS"],
            ["紫"] = ["maimai MURASAKi", "MURASAKi"],
            ["堇"] = ["maimai MURASAKi PLUS", "MURASAKi PLUS"],
            ["白"] = ["maimai MiLK", "MiLK"],
            ["雪"] = ["maimai MiLK PLUS", "MiLK PLUS"],
            ["辉"] = ["maimai FiNALE", "FiNALE"],
            ["熊"] = ["maimai でらっくす", "舞萌DX"],
            ["华"] = ["maimai でらっくす", "舞萌DX"],
            ["爽"] = ["maimai でらっくす Splash", "舞萌DX 2021"],
            ["煌"] = ["maimai でらっくす Splash", "舞萌DX 2021"],
            ["宙"] = ["maimai でらっくす UNiVERSE", "舞萌DX 2022"],
            ["星"] = ["maimai でらっくす UNiVERSE", "舞萌DX 2022"],
            ["祭"] = ["maimai でらっくす FESTiVAL", "舞萌DX 2023"],
            ["祝"] = ["maimai でらっくす FESTiVAL", "舞萌DX 2023"],
            ["双"] = ["maimai でらっくす BUDDiES", "舞萌DX 2024"],
            ["宴"] = ["maimai でらっくす BUDDiES", "舞萌DX 2024"],
            ["镜"] = ["maimai でらっくす PRiSM", "舞萌DX 2025"],
            ["彩"] = ["maimai でらっくす PRiSM", "舞萌DX 2025"],
            ["舞"] =
            [
                "maimai",
                "maimai PLUS",
                "maimai GreeN",
                "maimai GreeN PLUS",
                "maimai ORANGE",
                "maimai ORANGE PLUS",
                "maimai PiNK",
                "maimai PiNK PLUS",
                "maimai MURASAKi",
                "maimai MURASAKi PLUS",
                "maimai MiLK",
                "maimai MiLK PLUS",
                "maimai FiNALE",
                "GreeN",
                "GreeN PLUS",
                "ORANGE",
                "ORANGE PLUS",
                "PiNK",
                "PiNK PLUS",
                "MURASAKi",
                "MURASAKi PLUS",
                "MiLK",
                "MiLK PLUS",
                "FiNALE"
            ],
            ["霸"] =
            [
                "maimai",
                "maimai PLUS",
                "maimai GreeN",
                "maimai GreeN PLUS",
                "maimai ORANGE",
                "maimai ORANGE PLUS",
                "maimai PiNK",
                "maimai PiNK PLUS",
                "maimai MURASAKi",
                "maimai MURASAKi PLUS",
                "maimai MiLK",
                "maimai MiLK PLUS",
                "maimai FiNALE",
                "GreeN",
                "GreeN PLUS",
                "ORANGE",
                "ORANGE PLUS",
                "PiNK",
                "PiNK PLUS",
                "MURASAKi",
                "MURASAKi PLUS",
                "MiLK",
                "MiLK PLUS",
                "FiNALE"
            ]
        }.ToFrozenDictionary();

    public static (float, float, float) GetRatingFactors(Ranks rank) => RatingFactors[(int)rank];

    public static (Ranks, float) ResolveRankAndCoefficient(float achievements) => achievements switch
    {
        >= 101f => ((Ranks)14, 0.224f),
        >= 100.5f => (Ranks.SSSPlus, 0.224f),
        >= 100f => (Ranks.SSS, 0.216f),
        >= 99.5f => (Ranks.SSPlus, 0.211f),
        >= 99f => (Ranks.SS, 0.208f),
        >= 98f => (Ranks.SPlus, 0.203f),
        >= 97f => (Ranks.S, 0.2f),
        >= 94f => (Ranks.AAA, 0.168f),
        >= 90f => (Ranks.AA, 0.152f),
        >= 80f => (Ranks.A, 0.136f),
        >= 75f => (Ranks.BBB, 0.12f),
        >= 70f => (Ranks.BB, 0.112f),
        >= 60f => (Ranks.B, 0.96f),
        >= 50f => (Ranks.C, 0.8f),
        _ => (Ranks.D, 0.64f)
    };
}
