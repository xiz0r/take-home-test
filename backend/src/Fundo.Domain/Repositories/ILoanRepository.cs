using Fundo.Domain.Entities;

namespace Fundo.Domain.Repositories;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Loan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Loan> AddAsync(Loan loan, CancellationToken cancellationToken = default);
    Task UpdateAsync(Loan loan, CancellationToken cancellationToken = default);
    Task<IEnumerable<Loan>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount, CancellationToken cancellationToken = default);
}
