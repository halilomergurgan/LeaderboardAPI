using RuneGames.Application.Common.Exceptions;
using RuneGames.Application.Common.Interfaces;
using RuneGames.Application.Common.Models;
using RuneGames.Domain.Entities;
using RuneGames.Domain.Interfaces;

namespace RuneGames.Application.Features.Leaderboard.Commands;

public class SubmitScoreHandler
{
    private readonly ILeaderboardRepository _leaderboardRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILeaderboardCacheService _cache;
    private readonly IIdempotencyService _idempotency;

    public SubmitScoreHandler(
        ILeaderboardRepository leaderboardRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILeaderboardCacheService cache,
        IIdempotencyService idempotency)
    {
        _leaderboardRepository = leaderboardRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _idempotency = idempotency;
    }

    public async Task<Result<bool>> HandleAsync(SubmitScoreCommand command, CancellationToken ct = default)
    {
        if (await _idempotency.HasBeenProcessedAsync(command.IdempotencyKey, ct))
            throw new ValidationException("IdempotencyKey", "This request has already been processed.");

        if (command.Score < 0)
            throw new ValidationException("Score", "Score cannot be negative.");

        if (command.Score > 10_000_000)
            throw new ValidationException("Score", "Score exceeds maximum allowed value.");

        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user is null)
            throw new NotFoundException(nameof(User), command.UserId);

        var existing = await _leaderboardRepository.GetByUserIdAsync(command.UserId, ct);

        if (existing is null)
        {
            var entry = LeaderboardEntry.Create(command.UserId, command.Score, command.PlayerLevel, command.TrophyCount);
            await _leaderboardRepository.AddAsync(entry, ct);
        }
        else
        {
            existing.UpdateScore(command.Score, command.PlayerLevel, command.TrophyCount);
            await _leaderboardRepository.UpdateAsync(existing, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        await _idempotency.MarkAsProcessedAsync(command.IdempotencyKey, ct);
        await _cache.InvalidateAsync(ct);

        return Result<bool>.Success(true);
    }
}
