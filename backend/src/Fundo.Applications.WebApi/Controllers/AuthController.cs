using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fundo.Applications.WebApi.Authentication;
using Fundo.Applications.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fundo.Applications.WebApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly JwtOptions options;
    private readonly ILogger<AuthController> logger;

    public AuthController(IOptions<JwtOptions> options, ILogger<AuthController> logger)
    {
        this.options = options.Value;
        this.logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("token")]
    public ActionResult<AuthTokenResponse> CreateToken([FromBody] AuthTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            throw new ArgumentException("UserName is required.", nameof(request.UserName));
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.UserName),
            new Claim(JwtRegisteredClaimNames.UniqueName, request.UserName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(this.options.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: this.options.Issuer,
            audience: this.options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        this.logger.LogInformation("Issued token for {UserName}", request.UserName);

        return Ok(new AuthTokenResponse(
            tokenValue,
            (int)TimeSpan.FromMinutes(this.options.ExpirationMinutes).TotalSeconds));
    }
}
