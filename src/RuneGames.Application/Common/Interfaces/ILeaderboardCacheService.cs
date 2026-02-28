using RuneGames.Domain.Entities;

namespace RuneGames.Application.Common.Interfaces;

public interface ILeaderboardCacheService
{
    Task<List<LeaderboardEntry>?> GetTopAsync(CancellationToken ct = default);
    Task SetTopAsync(List<LeaderboardEntry> entries, CancellationToken ct = default);
    Task InvalidateAsync(CancellationToken ct = default);
}
