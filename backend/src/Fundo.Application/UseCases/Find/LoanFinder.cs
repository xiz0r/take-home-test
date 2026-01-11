using Fundo.Application.DTOs;
using Fundo.Application.UseCases.Shared;
using Fundo.Domain.Repositories;

namespace Fundo.Application.UseCases.Find;

public class LoanFinder
{
    private readonly ILoanRepository loanRepository;

    public LoanFinder(ILoanRepository loanRepository)
    {
        this.loanRepository = loanRepository;
    }

    public async Task<LoanResponse?> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var loan = await this.loanRepository.GetByIdAsync(id, cancellationToken);
        return loan is null ? null : LoanResponseMapper.Map(loan);
    }
}
