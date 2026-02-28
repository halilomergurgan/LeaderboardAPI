using RuneGames.Application.Common.Interfaces;
using RuneGames.Application.Common.Models;
using RuneGames.Domain.Entities;
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

    public async Task<Result<List<LeaderboardEntry>>> HandleAsync(GetTopPlayersQuery query, CancellationToken ct = default)
    {
        var cached = await _cache.GetTopAsync(ct);
        if (cached is not null)
            return Result<List<LeaderboardEntry>>.Success(cached);

        var entries = await _leaderboardRepository.GetTopNAsync(query.Count, ct);
        await _cache.SetTopAsync(entries, ct);

        return Result<List<LeaderboardEntry>>.Success(entries);
    }
}
