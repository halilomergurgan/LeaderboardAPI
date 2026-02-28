using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RuneGames.Application.Common.Interfaces;
using RuneGames.Domain.Interfaces;
using RuneGames.Infrastructure.Persistence;
using RuneGames.Infrastructure.Persistence.Repositories;
using RuneGames.Infrastructure.Services;

namespace RuneGames.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ILeaderboardCacheService, LeaderboardCacheService>();

        services.AddStackExchangeRedisCache(options =>
            options.Configuration = configuration.GetConnectionString("Redis"));

        return services;
    }
}
