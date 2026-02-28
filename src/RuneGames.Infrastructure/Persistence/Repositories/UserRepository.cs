using Microsoft.EntityFrameworkCore;
using RuneGames.Domain.Entities;
using RuneGames.Domain.Interfaces;

namespace RuneGames.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<bool> ExistsAsync(string username, CancellationToken ct = default)
        => await _context.Users.AnyAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);
}
