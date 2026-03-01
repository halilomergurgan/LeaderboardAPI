using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using RuneGames.Application.Common.Interfaces;

namespace RuneGames.Infrastructure.Services;

public class LeaderboardCacheService : ILeaderboardCacheService
{
    private readonly IDistributedCache _cache;
    private const string CacheKey = "leaderboard:top";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LeaderboardCacheService(IDistributedCache cache) => _cache = cache;

    public async Task<List<LeaderboardEntryCache>?> GetTopAsync(CancellationToken ct = default)
    {
        var json = await _cache.GetStringAsync(CacheKey, ct);
        if (json is null) return null;
        return JsonSerializer.Deserialize<List<LeaderboardEntryCache>>(json, _jsonOptions);
    }

    public async Task SetTopAsync(List<LeaderboardEntryCache> entries, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(entries, _jsonOptions);
        await _cache.SetStringAsync(CacheKey, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        }, ct);
    }

    public async Task InvalidateAsync(CancellationToken ct = default)
        => await _cache.RemoveAsync(CacheKey, ct);
}
