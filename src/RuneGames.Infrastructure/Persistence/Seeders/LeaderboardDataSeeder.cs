using Bogus;
using Microsoft.EntityFrameworkCore;
using RuneGames.Domain.Entities;
using RuneGames.Infrastructure.Persistence;

namespace RuneGames.Infrastructure.Persistence.Seeders;

public static class LeaderboardDataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var faker = new Faker();
        var random = new Random();
        var users = new List<User>();
        var entries = new List<LeaderboardEntry>();

        for (int i = 0; i < 100; i++)
        {
            var username = faker.Internet.UserName().Replace(".", "_").ToLower() + i;
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
            var deviceId = faker.Internet.Mac().Replace(":", "");

            var user = User.Create(username, passwordHash, deviceId);
            var entry = LeaderboardEntry.Create(
                userId: user.Id,
                score: random.Next(0, 100000),
                playerLevel: random.Next(1, 100),
                trophyCount: random.Next(0, 5000)
            );

            users.Add(user);
            entries.Add(entry);
        }

        await context.Users.AddRangeAsync(users);
        await context.LeaderboardEntries.AddRangeAsync(entries);
        await context.SaveChangesAsync();
    }
}
