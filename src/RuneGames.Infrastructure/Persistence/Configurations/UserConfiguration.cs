using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuneGames.Domain.Entities;

namespace RuneGames.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(u => u.DeviceId)
            .HasColumnName("device_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.RegistrationDate)
            .HasColumnName("registration_date")
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.HasOne(u => u.LeaderboardEntry)
            .WithOne(l => l.User)
            .HasForeignKey<LeaderboardEntry>(l => l.UserId);
    }
}
