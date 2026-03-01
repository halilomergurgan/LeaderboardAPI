using RuneGames.Application.Common.Interfaces;
using RuneGames.Application.Common.Models;
using RuneGames.Domain.Interfaces;

namespace RuneGames.Application.Features.Leaderboard.Queries;

public class GetTopPlayersHandler
{
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly ILeaderboardCacheService _cache;

    public GetTopPlayersHandler(ILeaderboardRepository leaderboardRepository, ILeaderboardCacheService cache)
    {
        _leaderboardRepository = leaderboardRepository;
        _cache = cache;
    }

    public async Task<Result<List<LeaderboardEntryCache>>> HandleAsync(GetTopPlayersQuery query, CancellationToken ct = default)
    {
        var cached = await _cache.GetTopAsync(ct);

        if (cached is not null && cached.Count >= query.Count)
            return Result<List<LeaderboardEntryCache>>.Success(cached.Take(query.Count).ToList());

        var entries = await _leaderboardRepository.GetTopNAsync(query.Count, ct);

        var cacheDtos = entries.Select(e => new LeaderboardEntryCache(
            e.Id,
            e.UserId,
            e.User.Username,
            e.Score,
            e.PlayerLevel,
            e.TrophyCount,
            e.LastUpdated
        )).ToList();

        await _cache.SetTopAsync(cacheDtos, ct);

        return Result<List<LeaderboardEntryCache>>.Success(cacheDtos.Take(query.Count).ToList());
    }
}
