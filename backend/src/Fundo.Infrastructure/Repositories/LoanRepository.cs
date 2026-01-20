using Fundo.Domain.Entities;
using Fundo.Domain.Repositories;
using Fundo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Infrastructure.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly FundoDbContext _context;

    public LoanRepository(FundoDbContext context)
    {
        _context = context;
    }

    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Loans.FindAsync([id], cancellationToken);
    }

    public async Task<IEnumerable<Loan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Loans.ToListAsync(cancellationToken);
    }

    public async Task<Loan> AddAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        await _context.Loans.AddAsync(loan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return loan;
    }

    public async Task UpdateAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Loan>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount, CancellationToken cancellationToken = default)
    {
        var loans = await _context.Loans
            .Where(loan => loan.Amount >= minAmount && loan.Amount <= maxAmount)
            .ToListAsync(cancellationToken);

        return loans;
    }
}
