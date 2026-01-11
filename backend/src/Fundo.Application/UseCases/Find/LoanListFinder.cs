using Fundo.Application.DTOs;
using Fundo.Application.UseCases.Shared;
using Fundo.Domain.Repositories;

namespace Fundo.Application.UseCases.Find;

public class LoanListFinder
{
    private readonly ILoanRepository loanRepository;

    public LoanListFinder(ILoanRepository loanRepository)
    {
        this.loanRepository = loanRepository;
    }

    public async Task<IEnumerable<LoanResponse>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var loans = await this.loanRepository.GetAllAsync(cancellationToken);
        return loans.Select(LoanResponseMapper.Map);
    }
}
