using RuneGames.Application.Common.Exceptions;
using RuneGames.Application.Common.Interfaces;
using RuneGames.Application.Common.Models;
using RuneGames.Domain.Interfaces;

namespace RuneGames.Application.Features.Auth.Commands;

public class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public LoginHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> HandleAsync(LoginCommand command, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(command.Username, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            throw new ValidationException("Credentials", "Invalid username or password.");

        var token = _jwtService.GenerateToken(user);
        return Result<string>.Success(token);
    }
}
