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
            .ThenBy(l => l.User.RegistrationDate)
            .ThenByDescending(l => l.PlayerLevel)
            .ThenByDescending(l => l.TrophyCount)
            .Take(count)
            .ToListAsync(ct);

    public async Task<int> GetUserRankAsync(Guid userId, CancellationToken ct = default)
    {
        var userEntry = await _context.LeaderboardEntries
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.UserId == userId, ct);

        if (userEntry is null) return 0;

        var rank = await _context.LeaderboardEntries
            .Include(l => l.User)
            .CountAsync(l =>
                l.Score > userEntry.Score ||
                (l.Score == userEntry.Score && l.User.RegistrationDate < userEntry.User.RegistrationDate) ||
                (l.Score == userEntry.Score && l.User.RegistrationDate == userEntry.User.RegistrationDate && l.PlayerLevel > userEntry.PlayerLevel) ||
                (l.Score == userEntry.Score && l.User.RegistrationDate == userEntry.User.RegistrationDate && l.PlayerLevel == userEntry.PlayerLevel && l.TrophyCount > userEntry.TrophyCount),
            ct);

        return rank + 1;
    }

    public async Task<List<LeaderboardEntry>> GetSurroundingEntriesAsync(Guid userId, int range, CancellationToken ct = default)
    {
        var rank = await GetUserRankAsync(userId, ct);
        if (rank == 0) return new List<LeaderboardEntry>();

        return await _context.LeaderboardEntries
            .Include(l => l.User)
            .OrderByDescending(l => l.Score)
            .ThenBy(l => l.User.RegistrationDate)
            .ThenByDescending(l => l.PlayerLevel)
            .ThenByDescending(l => l.TrophyCount)
            .Skip(Math.Max(0, rank - range - 1))
            .Take(range * 2 + 1)
            .ToListAsync(ct);
    }
}
