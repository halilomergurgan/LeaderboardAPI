using Microsoft.EntityFrameworkCore;
using RuneGames.Domain.Entities;
using RuneGames.Domain.Interfaces;

namespace RuneGames.Infrastructure.Persistence.Repositories;

public class LeaderboardRepository : ILeaderboardRepository
{
    private readonly AppDbContext _context;

    public LeaderboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LeaderboardEntry?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.LeaderboardEntries.FirstOrDefaultAsync(l => l.UserId == userId, ct);

    public async Task AddAsync(LeaderboardEntry entry, CancellationToken ct = default)
        => await _context.LeaderboardEntries.AddAsync(entry, ct);

    public Task UpdateAsync(LeaderboardEntry entry, CancellationToken ct = default)
    {
        _context.LeaderboardEntries.Update(entry);
        return Task.CompletedTask;
    }

    public async Task<List<LeaderboardEntry>> GetTopNAsync(int count, CancellationToken ct = default)
        => await _context.LeaderboardEntries
            .Include(l => l.User)
            .OrderByDescending(l => l.Score)
            .Take(count)
            .ToListAsync(ct);

    public async Task<int> GetUserRankAsync(Guid userId, CancellationToken ct = default)
    {
        var userScore = await _context.LeaderboardEntries
            .Where(l => l.UserId == userId)
            .Select(l => l.Score)
            .FirstOrDefaultAsync(ct);

        if (userScore == 0) return 0;

        var rank = await _context.LeaderboardEntries
            .CountAsync(l => l.Score > userScore, ct);

        return rank + 1;
    }

    public async Task<List<LeaderboardEntry>> GetSurroundingEntriesAsync(Guid userId, int range, CancellationToken ct = default)
    {
        var rank = await GetUserRankAsync(userId, ct);
        if (rank == 0) return new List<LeaderboardEntry>();

        return await _context.LeaderboardEntries
            .Include(l => l.User)
            .OrderByDescending(l => l.Score)
            .Skip(Math.Max(0, rank - range - 1))
            .Take(range * 2 + 1)
            .ToListAsync(ct);
    }
}
