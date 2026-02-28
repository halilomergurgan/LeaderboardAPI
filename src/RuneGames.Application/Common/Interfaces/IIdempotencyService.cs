namespace RuneGames.Application.Common.Interfaces;

public interface IIdempotencyService
{
    Task<bool> HasBeenProcessedAsync(Guid key, CancellationToken ct = default);
    Task MarkAsProcessedAsync(Guid key, CancellationToken ct = default);
}
