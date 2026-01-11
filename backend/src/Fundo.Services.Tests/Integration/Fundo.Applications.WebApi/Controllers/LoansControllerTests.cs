using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Fundo.Application.DTOs;
using Fundo.Services.Tests.Integration.Support;
using Fundo.Services.Tests.Shared.ObjectMothers;
using Xunit;

namespace Fundo.Services.Tests.Integration;

public class LoansControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly LoanApiClient loanApi;

    public LoansControllerTests(TestWebApplicationFactory factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        this.loanApi = new LoanApiClient(client);
    }

    [Fact]
    public async Task CreateLoan_ShouldReturnCreatedWithLoan()
    {
        var response = await this.loanApi.CreateLoanAsync(
            LoanRequestMother.CreateLoanRequest(500m, "Test Applicant"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var loan = await this.loanApi.ReadLoanAsync(response);
        Assert.NotNull(loan);
        Assert.Equal(500m, loan!.Amount);
        Assert.Equal(500m, loan.CurrentBalance);
        Assert.Equal("Test Applicant", loan.ApplicantName);
        Assert.Equal("active", loan.Status);
    }

    [Fact]
    public async Task GetLoanById_ShouldReturnLoan()
    {
        var created = await CreateLoanAsync(350m, "Get By Id");

        var response = await this.loanApi.GetLoanAsync(created.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loan = await this.loanApi.ReadLoanAsync(response);
        Assert.NotNull(loan);
        Assert.Equal(created.Id, loan!.Id);
        Assert.Equal(created.Amount, loan.Amount);
        Assert.Equal(created.ApplicantName, loan.ApplicantName);
    }

    [Fact]
    public async Task GetLoanById_WhenMissing_ShouldReturnNotFound()
    {
        var response = await this.loanApi.GetLoanAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllLoans_ShouldReturnOk()
    {
        var created = await CreateLoanAsync(420m, "List Test");

        var response = await this.loanApi.GetLoansAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loans = await this.loanApi.ReadLoansAsync(response);
        Assert.NotNull(loans);
        Assert.Contains(loans!, loan => loan.Id == created.Id);
    }

    [Fact]
    public async Task MakePayment_ShouldReturnUpdatedLoan()
    {
        var created = await CreateLoanAsync(200m, "Payment Test");

        var response = await this.loanApi.MakePaymentAsync(
            created.Id,
            LoanRequestMother.MakePaymentRequest(50m));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loan = await this.loanApi.ReadLoanAsync(response);
        Assert.NotNull(loan);
        Assert.Equal(150m, loan!.CurrentBalance);
        Assert.Equal("active", loan.Status);
        Assert.NotNull(loan.UpdatedAt);
    }

    private async Task<LoanResponse> CreateLoanAsync(decimal amount, string applicantName)
    {
        var response = await this.loanApi.CreateLoanAsync(
            LoanRequestMother.CreateLoanRequest(amount, applicantName));

        response.EnsureSuccessStatusCode();

        var loan = await this.loanApi.ReadLoanAsync(response);
        Assert.NotNull(loan);
        return loan!;
    }
}
