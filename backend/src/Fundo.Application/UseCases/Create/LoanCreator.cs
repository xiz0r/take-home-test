using Fundo.Application.DTOs;
using Fundo.Application.UseCases.Shared;
using Fundo.Domain.Entities;
using Fundo.Domain.Repositories;

namespace Fundo.Application.UseCases.Create;

public class LoanCreator
{
    private readonly ILoanRepository loanRepository;

    public LoanCreator(ILoanRepository loanRepository)
    {
        this.loanRepository = loanRepository;
    }

    public async Task<LoanResponse> ExecuteAsync(
        CreateLoanRequest request,
        CancellationToken cancellationToken = default)
    {
        var loan = Loan.Create(request.Amount, request.ApplicantName);
        await this.loanRepository.AddAsync(loan, cancellationToken);
        return LoanResponseMapper.Map(loan);
    }
}
