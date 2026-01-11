using System.Net.Http.Json;
using Fundo.Application.DTOs;

namespace Fundo.Services.Tests.Integration.Support;

public class LoanApiClient
{
    private readonly HttpClient client;

    public LoanApiClient(HttpClient client)
    {
        this.client = client;
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
