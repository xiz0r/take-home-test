using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fundo.Application.DTOs;
using Fundo.Applications.WebApi.DTOs;

namespace Fundo.Services.Tests.Integration.Support;

public class LoanApiClient
{
    private readonly HttpClient client;
    private bool isAuthenticated;

    public LoanApiClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task AuthenticateAsync(AuthTokenRequest? request = null)
    {
        if (this.isAuthenticated)
        {
            return;
        }

        var response = await this.client.PostAsJsonAsync(
            "/auth/token",
            request ?? new AuthTokenRequest("test-user"));

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new InvalidOperationException("Failed to acquire auth token.");
        }

        this.client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
        this.isAuthenticated = true;
    }

    public Task<HttpResponseMessage> CreateLoanAsync(CreateLoanRequest request)
    {
        return this.client.PostAsJsonAsync("/loans", request);
    }

    public Task<HttpResponseMessage> GetLoanAsync(Guid id)
    {
        return this.client.GetAsync($"/loans/{id}");
    }

    public Task<HttpResponseMessage> GetLoansAsync()
    {
        return this.client.GetAsync("/loans");
    }

    public Task<HttpResponseMessage> MakePaymentAsync(Guid id, MakePaymentRequest request)
    {
        return this.client.PostAsJsonAsync($"/loans/{id}/payment", request);
    }

    public async Task<LoanResponse?> ReadLoanAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<LoanResponse>();
    }

    public async Task<List<LoanResponse>?> ReadLoansAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<List<LoanResponse>>();
    }
}
