namespace RuneGames.Domain.Entities;

public class LeaderboardEntry
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public long Score { get; private set; }
    public int PlayerLevel { get; private set; }
    public int TrophyCount { get; private set; }
    public DateTime LastUpdated { get; private set; }
    public User User { get; private set; } = null!;

    private LeaderboardEntry() { }

    public static LeaderboardEntry Create(Guid userId, long score, int playerLevel, int trophyCount)
    {
        return new LeaderboardEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Score = score,
            PlayerLevel = playerLevel,
            TrophyCount = trophyCount,
            LastUpdated = DateTime.UtcNow
        };
    }

    public void UpdateScore(long newScore, int playerLevel, int trophyCount)
    {
        if (newScore > Score)
            Score = newScore;

        PlayerLevel = playerLevel;
        TrophyCount = trophyCount;
        LastUpdated = DateTime.UtcNow;
    }
}
