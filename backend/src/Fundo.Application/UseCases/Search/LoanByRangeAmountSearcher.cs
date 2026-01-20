
using Fundo.Application.DTOs;
using Fundo.Application.UseCases.Shared;
using Fundo.Domain.Repositories;

namespace Fundo.Application.UseCases.Search;

public class LoanByRangeAmountSearcher
{

  private readonly ILoanRepository loanRepository;
  public LoanByRangeAmountSearcher(ILoanRepository loanRepository)
  {
    this.loanRepository = loanRepository;
  }

  public async Task<IEnumerable<LoanResponse>> ExecuteAsync(SearchByAmountRangeRequest request, CancellationToken cancellationToken = default)
  {
    var loans = await this.loanRepository.GetByAmountRangeAsync(request.MinAmount, request.MaxAmount, cancellationToken);
    return loans.Select(LoanResponseMapper.Map);
  }
}