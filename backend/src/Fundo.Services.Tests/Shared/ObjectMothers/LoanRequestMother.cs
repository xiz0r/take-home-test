using Fundo.Application.DTOs;

namespace Fundo.Services.Tests.Shared.ObjectMothers;

public static class LoanRequestMother
{
    public static CreateLoanRequest CreateLoanRequest(
        decimal amount = 500m,
        string applicantName = "Test Applicant")
    {
        return new CreateLoanRequest(amount, applicantName);
    }

    public static MakePaymentRequest MakePaymentRequest(decimal amount = 50m)
    {
        return new MakePaymentRequest(amount);
    }

    public static SearchByAmountRangeRequest CreateSearchByAmountRangeRequest(
        decimal minAmount = 100m,
        decimal maxAmount = 1000m)
    {
        return new SearchByAmountRangeRequest(minAmount, maxAmount);
    }
}
