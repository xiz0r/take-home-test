using Fundo.Domain.Enums;

namespace Fundo.Domain.Entities;

public class Loan
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public string ApplicantName { get; private set; } = string.Empty;
    public LoanStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Loan() { }

    public static Loan Create(decimal amount, string applicantName)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        if (string.IsNullOrWhiteSpace(applicantName))
            throw new ArgumentException("Applicant name is required", nameof(applicantName));

        return new Loan
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            CurrentBalance = amount,
            ApplicantName = applicantName,
            Status = LoanStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MakePayment(decimal paymentAmount)
    {
        if (paymentAmount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero", nameof(paymentAmount));

        if (paymentAmount > CurrentBalance)
            throw new InvalidOperationException("Payment amount exceeds current balance");

        if (Status == LoanStatus.Paid)
            throw new InvalidOperationException("Cannot make payment on a paid loan");

        CurrentBalance -= paymentAmount;
        UpdatedAt = DateTime.UtcNow;

        if (CurrentBalance == 0)
        {
            Status = LoanStatus.Paid;
        }
    }
}
