namespace RuneGames.Application.Features.Leaderboard.Commands;

public record SubmitScoreCommand(Guid UserId, long Score, int PlayerLevel, int TrophyCount, Guid IdempotencyKey);
