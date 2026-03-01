using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RuneGames.Application.Common.Messages;
using RuneGames.Application.Features.Leaderboard.Commands;

namespace RuneGames.Infrastructure.Messaging;

public class ScoreConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScoreConsumer> _logger;
    private readonly ConnectionFactory _factory;
    private readonly string _queueName;
    private IConnection? _connection;
    private IChannel? _channel;

    public ScoreConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<ScoreConsumer> logger)
    {
        _serviceProvider = serviceProvider;
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Received message from queue: {Body}", body);

            try
            {
                var evt = JsonSerializer.Deserialize<ScoreSubmittedEvent>(body);
                if (evt is null)
                {
                    await _channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<SubmitScoreHandler>();

                await handler.HandleAsync(new SubmitScoreCommand(
                    evt.UserId,
                    evt.Score,
                    evt.PlayerLevel,
                    evt.TrophyCount,
                    evt.IdempotencyKey
                ), stoppingToken);

                await _channel!.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                _logger.LogInformation("Score processed for user {UserId}", evt.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process score message");
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await _channel!.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ConnectWithRetryAsync(CancellationToken ct)
    {
        const int maxRetries = 10;
        const int delaySeconds = 3;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Connecting to RabbitMQ (attempt {Attempt}/{Max})...", attempt, maxRetries);

                _connection = await _factory.CreateConnectionAsync(ct);
                _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

                await _channel.QueueDeclareAsync(
                    queue: _queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: ct
                );

                _logger.LogInformation("Connected to RabbitMQ successfully.");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning("RabbitMQ not ready (attempt {Attempt}): {Message}. Retrying in {Delay}s...",
                    attempt, ex.Message, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
            }
        }

        throw new Exception("Could not connect to RabbitMQ after maximum retries.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}
