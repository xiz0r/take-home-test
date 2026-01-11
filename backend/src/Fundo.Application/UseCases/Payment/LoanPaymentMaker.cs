using Fundo.Application.DTOs;
using Fundo.Application.UseCases.Shared;
using Fundo.Domain.Repositories;

namespace Fundo.Application.UseCases.Payment;

public class LoanPaymentMaker
{
    private readonly ILoanRepository loanRepository;

    public LoanPaymentMaker(ILoanRepository loanRepository)
    {
        this.loanRepository = loanRepository;
    }

    public async Task<LoanResponse> ExecuteAsync(
        Guid loanId,
        MakePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var loan = await this.loanRepository.GetByIdAsync(loanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan with id {loanId} not found");

        loan.MakePayment(request.Amount);
        await this.loanRepository.UpdateAsync(loan, cancellationToken);

        return LoanResponseMapper.Map(loan);
    }
}
