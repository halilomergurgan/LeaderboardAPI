namespace RuneGames.Application.Common.Messages;

public record ScoreSubmittedEvent(
    Guid UserId,
    long Score,
    int PlayerLevel,
    int TrophyCount,
    Guid IdempotencyKey
);
