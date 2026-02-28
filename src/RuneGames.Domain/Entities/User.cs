namespace RuneGames.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string DeviceId { get; private set; } = string.Empty;
    public DateTime RegistrationDate { get; private set; }
    public LeaderboardEntry? LeaderboardEntry { get; private set; }

    private User() { }

    public static User Create(string username, string passwordHash, string deviceId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHash,
            DeviceId = deviceId,
            RegistrationDate = DateTime.UtcNow
        };
    }
}
