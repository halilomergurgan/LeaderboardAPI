using RuneGames.Domain.Entities;

namespace RuneGames.Domain.Interfaces;

public interface ILeaderboardRepository
{
    Task<LeaderboardEntry?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(LeaderboardEntry entry, CancellationToken ct = default);
    Task UpdateAsync(LeaderboardEntry entry, CancellationToken ct = default);
    Task<List<LeaderboardEntry>> GetTopNAsync(int count, CancellationToken ct = default);
    Task<int> GetUserRankAsync(Guid userId, CancellationToken ct = default);
    Task<List<LeaderboardEntry>> GetSurroundingEntriesAsync(Guid userId, int range, CancellationToken ct = default);
}
