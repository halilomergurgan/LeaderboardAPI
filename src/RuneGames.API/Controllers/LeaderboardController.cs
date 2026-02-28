using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuneGames.Application.Common.Exceptions;
using RuneGames.Application.Features.Leaderboard.Commands;
using RuneGames.Application.Features.Leaderboard.Queries;

namespace RuneGames.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly GetTopPlayersHandler _getTopPlayersHandler;
    private readonly GetPlayerRankHandler _getPlayerRankHandler;
    private readonly SubmitScoreHandler _submitScoreHandler;

    public LeaderboardController(
        GetTopPlayersHandler getTopPlayersHandler,
        GetPlayerRankHandler getPlayerRankHandler,
        SubmitScoreHandler submitScoreHandler)
    {
        _getTopPlayersHandler = getTopPlayersHandler;
        _getPlayerRankHandler = getPlayerRankHandler;
        _submitScoreHandler = submitScoreHandler;
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
    public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _submitScoreHandler.HandleAsync(command, ct);
            return Ok(new { success = result.Data });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
