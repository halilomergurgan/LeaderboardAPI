using RuneGames.Application.Common.Exceptions;
using RuneGames.Application.Common.Models;
using RuneGames.Domain.Entities;
using RuneGames.Domain.Interfaces;

namespace RuneGames.Application.Features.Auth.Commands;

public class RegisterHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(RegisterCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || command.Username.Length < 3)
            throw new ValidationException("Username", "Username must be at least 3 characters.");

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 6)
            throw new ValidationException("Password", "Password must be at least 6 characters.");

        if (await _userRepository.ExistsAsync(command.Username, ct))
            throw new ValidationException("Username", "Username is already taken.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
        var user = User.Create(command.Username, passwordHash, command.DeviceId);

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<Guid>.Success(user.Id);
    }
}
