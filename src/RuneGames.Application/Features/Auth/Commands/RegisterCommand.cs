namespace RuneGames.Application.Features.Auth.Commands;

public record RegisterCommand(string Username, string Password, string DeviceId);
