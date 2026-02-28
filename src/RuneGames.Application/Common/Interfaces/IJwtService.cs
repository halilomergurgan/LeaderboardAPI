using RuneGames.Domain.Entities;

namespace RuneGames.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
