using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneGames.Domain.Entities;

namespace RuneGames.Infrastructure.Persistence.Configurations;

public class LeaderboardEntryConfiguration : IEntityTypeConfiguration<LeaderboardEntry>
{
    public void Configure(EntityTypeBuilder<LeaderboardEntry> builder)
    {
        builder.ToTable("leaderboard_entries");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.Property(l => l.UserId)
            .HasColumnName("user_id");

        builder.Property(l => l.Score)
            .HasColumnName("score")
            .IsRequired();

        builder.Property(l => l.PlayerLevel)
            .HasColumnName("player_level")
            .IsRequired();

        builder.Property(l => l.TrophyCount)
            .HasColumnName("trophy_count")
            .IsRequired();

        builder.Property(l => l.LastUpdated)
            .HasColumnName("last_updated")
            .IsRequired();

        builder.HasIndex(l => l.Score)
            .IsDescending();

        builder.HasIndex(l => l.UserId)
            .IsUnique();
    }
}
