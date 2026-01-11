namespace Fundo.Applications.WebApi.DTOs;

public record AuthTokenResponse(string AccessToken, int ExpiresInSeconds);
