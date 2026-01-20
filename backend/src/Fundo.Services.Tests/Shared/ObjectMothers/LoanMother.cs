using Fundo.Domain.Entities;

namespace Fundo.Services.Tests.Shared.ObjectMothers;

public static class LoanMother
{
    public static Loan ActiveLoan(
        decimal amount = 500m,
        string applicantName = "Test Applicant")
    {
        return Loan.Create(amount, applicantName);
    }

    public static Loan PaidLoan(
        decimal amount = 500m,
        string applicantName = "Test Applicant")
    {
        var loan = Loan.Create(amount, applicantName);
        loan.MakePayment(amount);
        return loan;
    }

    public static Loan LoanWithBalance(
        decimal amount,
        decimal currentBalance,
        string applicantName = "Test Applicant")
    {
        var loan = Loan.Create(amount, applicantName);

        if (currentBalance < amount)
        {
            var paymentAmount = amount - currentBalance;
            loan.MakePayment(paymentAmount);
        }

        return loan;
    }
}
