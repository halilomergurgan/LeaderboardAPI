using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using RuneGames.Application.Common.Interfaces;
using RuneGames.Domain.Entities;

namespace RuneGames.Infrastructure.Services;

public class LeaderboardCacheService : ILeaderboardCacheService
{
    private readonly IDistributedCache _cache;
    private const string CacheKey = "leaderboard:top";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public LeaderboardCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<List<LeaderboardEntry>?> GetTopAsync(CancellationToken ct = default)
    {
        var json = await _cache.GetStringAsync(CacheKey, ct);
        if (json is null) return null;

        return JsonSerializer.Deserialize<List<LeaderboardEntry>>(json);
    }

    public async Task SetTopAsync(List<LeaderboardEntry> entries, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(entries);
        await _cache.SetStringAsync(CacheKey, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        }, ct);
    }

    public async Task InvalidateAsync(CancellationToken ct = default)
        => await _cache.RemoveAsync(CacheKey, ct);
}
