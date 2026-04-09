namespace Limekuma.Prober.Common;

public class CommonSong
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public required CommonSongTypes Type { get; init; }

    public required string Genre { get; init; }

    public required bool InCurrentGenre { get; init; }

    public required string AudioUrl { get; init; }

    public required string JacketUrl { get; init; }
}
