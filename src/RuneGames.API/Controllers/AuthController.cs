using Microsoft.AspNetCore.Mvc;
using RuneGames.Application.Common.Exceptions;
using RuneGames.Application.Features.Auth.Commands;

namespace RuneGames.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterHandler _registerHandler;
    private readonly LoginHandler _loginHandler;

    public AuthController(RegisterHandler registerHandler, LoginHandler loginHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _registerHandler.HandleAsync(command, ct);
            return Ok(new { userId = result.Data });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        try
        {
            var result = await _loginHandler.HandleAsync(command, ct);
            return Ok(new { token = result.Data });
        }
        catch (ValidationException ex)
        {
            return Unauthorized(new { errors = ex.Errors });
        }
    }
}
