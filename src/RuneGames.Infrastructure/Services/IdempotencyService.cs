using Microsoft.Extensions.Caching.Distributed;
using RuneGames.Application.Common.Interfaces;

namespace RuneGames.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan Expiry = TimeSpan.FromHours(24);

    public IdempotencyService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> HasBeenProcessedAsync(Guid key, CancellationToken ct = default)
    {
        var value = await _cache.GetStringAsync($"idempotency:{key}", ct);
        return value is not null;
    }

    public async Task MarkAsProcessedAsync(Guid key, CancellationToken ct = default)
    {
        await _cache.SetStringAsync(
            $"idempotency:{key}",
            "1",
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Expiry },
            ct);
    }
}
