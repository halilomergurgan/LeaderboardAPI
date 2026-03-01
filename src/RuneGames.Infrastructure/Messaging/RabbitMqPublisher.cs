using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RuneGames.Application.Common.Interfaces;

namespace RuneGames.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private readonly string _queueName;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized = false;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _queueName = configuration["RabbitMq:QueueName"]!;

        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"]!,
            Port = int.Parse(configuration["RabbitMq:Port"]!),
            UserName = configuration["RabbitMq:Username"]!,
            Password = configuration["RabbitMq:Password"]!
        };
    }

    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (_initialized) return;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_initialized) return;

            _connection = await _factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: ct
            );

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
    {
        await EnsureInitializedAsync(ct);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties { Persistent = true };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _queueName,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct
        );

        _logger.LogInformation("Published message to queue '{Queue}': {Message}", _queueName, json);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        _initLock.Dispose();
    }
}
