namespace RuneGames.Application.Common.Interfaces;

public record LeaderboardEntryCache(
    Guid Id,
    Guid UserId,
    string Username,
    long Score,
    int PlayerLevel,
    int TrophyCount,
    string LastUpdated,
    int Rank
);

public interface ILeaderboardCacheService
{
    Task<List<LeaderboardEntryCache>?> GetTopAsync(CancellationToken ct = default);
    Task SetTopAsync(List<LeaderboardEntryCache> entries, CancellationToken ct = default);
    Task InvalidateAsync(CancellationToken ct = default);
}
