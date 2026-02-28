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

        var result = new List<RankEntry>();
        foreach (var entry in surrounding)
        {
            var user = await _userRepository.GetByIdAsync(entry.UserId, ct);
            var entryRank = await _leaderboardRepository.GetUserRankAsync(entry.UserId, ct);
            result.Add(new RankEntry(entry.UserId, user?.Username ?? "Unknown", entry.Score, entryRank));
        }

        return Result<PlayerRankResult>.Success(new PlayerRankResult(rank, result));
    }
}
