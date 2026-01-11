using Fundo.Applications.WebApi.DTOs;

namespace Fundo.Services.Tests.Shared.ObjectMothers;

public static class AuthRequestMother
{
    public static AuthTokenRequest CreateTokenRequest(string userName = "test-user")
    {
        return new AuthTokenRequest(userName);
    }
}
