namespace RuneGames.Application.Features.Leaderboard.Queries;

public record GetPlayerRankQuery(Guid UserId, int SurroundingRange = 5);
