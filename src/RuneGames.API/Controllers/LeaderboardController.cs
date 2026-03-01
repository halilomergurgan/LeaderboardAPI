using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuneGames.Application.Common.Exceptions;
using RuneGames.Application.Common.Interfaces;
using RuneGames.Application.Common.Messages;
using RuneGames.Application.Features.Leaderboard.Commands;
using RuneGames.Application.Features.Leaderboard.Queries;
using RuneGames.Domain.Interfaces;
using System.Security.Claims;

namespace RuneGames.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly GetTopPlayersHandler _getTopPlayersHandler;
    private readonly GetPlayerRankHandler _getPlayerRankHandler;
    private readonly IMessagePublisher _publisher;
    private readonly IUserRepository _userRepository;

    public LeaderboardController(
        GetTopPlayersHandler getTopPlayersHandler,
        GetPlayerRankHandler getPlayerRankHandler,
        IMessagePublisher publisher,
        IUserRepository userRepository)
    {
        _getTopPlayersHandler = getTopPlayersHandler;
        _getPlayerRankHandler = getPlayerRankHandler;
        _publisher = publisher;
        _userRepository = userRepository;
    }

    [HttpGet("top")]
    public async Task<IActionResult> GetTopPlayers([FromQuery] int count = 100, CancellationToken ct = default)
    {
        var result = await _getTopPlayersHandler.HandleAsync(new GetTopPlayersQuery(count), ct);
        return Ok(result.Data);
    }

    [HttpGet("rank/{userId:guid}")]
    public async Task<IActionResult> GetPlayerRank(Guid userId, [FromQuery] int surroundingRange = 5, CancellationToken ct = default)
    {
        try
        {
            var result = await _getPlayerRankHandler.HandleAsync(new GetPlayerRankQuery(userId, surroundingRange), ct);
            return Ok(result.Data);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("score")]
    public async Task<IActionResult> SubmitScore(
    [FromBody] SubmitScoreCommand command,
    [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
    CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null)
            return NotFound(new { message = "User not found." });

        try
        {
            await _publisher.PublishAsync(new ScoreSubmittedEvent(
                userId,
                command.Score,
                command.PlayerLevel,
                command.TrophyCount,
                idempotencyKey
            ), ct);

            return Accepted(new { message = "Score submission received and queued for processing." });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }
}
