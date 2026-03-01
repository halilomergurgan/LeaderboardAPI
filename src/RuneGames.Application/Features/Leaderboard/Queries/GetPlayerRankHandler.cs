using RuneGames.Application.Common.Exceptions;
using RuneGames.Application.Common.Models;
using RuneGames.Domain.Interfaces;

namespace RuneGames.Application.Features.Leaderboard.Queries;

public record PlayerRankResult(int Rank, List<RankEntry> Surrounding);
public record RankEntry(Guid UserId, string Username, long Score, int Rank);

public class GetPlayerRankHandler
{
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly IUserRepository _userRepository;

    public GetPlayerRankHandler(ILeaderboardRepository leaderboardRepository, IUserRepository userRepository)
    {
        _leaderboardRepository = leaderboardRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<PlayerRankResult>> HandleAsync(GetPlayerRankQuery query, CancellationToken ct = default)
    {
        var rank = await _leaderboardRepository.GetUserRankAsync(query.UserId, ct);
        if (rank == 0)
            throw new NotFoundException("LeaderboardEntry", query.UserId);

        var surrounding = await _leaderboardRepository.GetSurroundingEntriesAsync(query.UserId, query.SurroundingRange, ct);

        var startRank = Math.Max(1, rank - query.SurroundingRange);

        var result = surrounding
            .Select((entry, index) => new RankEntry(
                entry.UserId,
                entry.User?.Username ?? "Unknown",
                entry.Score,
                startRank + index
            ))
            .ToList();

        return Result<PlayerRankResult>.Success(new PlayerRankResult(rank, result));
    }

}
