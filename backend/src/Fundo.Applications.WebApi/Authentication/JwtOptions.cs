namespace Fundo.Applications.WebApi.Authentication;

public class JwtOptions
{
    public string Issuer { get; set; } = "Fundo";
    public string Audience { get; set; } = "FundoWeb";
    public string Secret { get; set; } = "fundo-dev-secret-please-change-1234567890";
    public int ExpirationMinutes { get; set; } = 60;
}
