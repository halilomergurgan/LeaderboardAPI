namespace RuneGames.Application.Common.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class;
}
